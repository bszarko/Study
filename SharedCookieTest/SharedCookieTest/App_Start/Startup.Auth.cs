using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Infrastructure;
using Microsoft.AspNetCore.DataProtection.Internal;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Interop;
using Microsoft.Win32;
using Owin;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Web.Hosting;


namespace SharedCookieTest
{
	public partial class Startup
	{
		// For more information on configuring authentication, please visit https://go.microsoft.com/fwlink/?LinkId=301864
		public void ConfigureAuth(IAppBuilder app)
		{
			// these don't seem to change the behavior
			if (CryptoConfig.AllowOnlyFipsAlgorithms)
			{
				CryptoConfig.AddAlgorithm(
					typeof(AesCryptoServiceProvider),
					"AES",
					"AesCryptoServiceProvider",
					"System.Security.Cryptography.AesCryptoServiceProvider",
					"System.Security.Cryptography.AES"
				);

				CryptoConfig.AddAlgorithm(
					typeof(SHA256CryptoServiceProvider),
					"SHA256",
					"SHA256CryptoServiceProvider",
					"System.Security.Cryptography.SHA256CryptoServiceProvider",
					"System.Security.Cryptography.SHA256"
				);

				CryptoConfig.AddAlgorithm(
					typeof(SHA256CryptoServiceProvider),
					"SHA512",
					"SHA512CryptoServiceProvider",
					"System.Security.Cryptography.SHA512CryptoServiceProvider",
					"System.Security.Cryptography.SHA512"
				);
			}

			var encryptionSettings = new AuthenticatedEncryptorConfiguration()
			{
				EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
				ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
			};

			// apply AppPool identity or service account permissions to registry path
			var regPath = @"SOFTWARE\SharedCookieTest\Keys";
			Registry.LocalMachine.CreateSubKey(regPath, true);

			/*
			 * Certificate Thumbprints
			 * 6bee3f190055703c0dcd01e7c433462069549762 - dev.sharedcookietest.cert02
			 * d74e3a5413047996a10bda4661d296ca7f741ff8 - Dev.SharedCookieTest.Cert01
			*/

			var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
			store.Open(OpenFlags.OpenExistingOnly);
			var cert = store.Certificates
				.OfType<X509Certificate2>()
				.FirstOrDefault(c => c.Thumbprint.Equals("6bee3f190055703c0dcd01e7c433462069549762", StringComparison.CurrentCultureIgnoreCase));

			var protectionProvider = DataProtectionProvider.Create(
				new DirectoryInfo($@"{HostingEnvironment.MapPath("~")}\keys"),
				options =>
				{
					options.SetDefaultKeyLifetime(TimeSpan.FromDays(365));
					options.UseCryptographicAlgorithms(encryptionSettings);

					//options.UseCustomCryptographicAlgorithms(
					//	new CngCbcAuthenticatedEncryptorConfiguration()
					//	{
					//		EncryptionAlgorithm = "AES",
					//		//EncryptionAlgorithmProvider = WhatShouldThisBe?,//"Advanced Encryption Standard",//"ECDiffieHellmanCng",//"BCryptCloseAlgorithmProvider",//"MS_ENH_RSA_AES_PROV",
					//		EncryptionAlgorithmKeySize = 256,
					//		HashAlgorithm = "SHA256"
					//		//HashAlgorithmProvider = WhatShouldThisBe?//null//"SHA256CryptoServiceProvider"//"Microsoft Enhanced RSA and AES Cryptographic Provider"
					//	});

					options.PersistKeysToRegistry(Registry.LocalMachine.OpenSubKey(regPath, true));
					options.ProtectKeysWithCertificate(cert);
				});

			var dataProtector = protectionProvider.CreateProtector(
				"CookieAuthenticationMiddleware",
				"Cookie",
				"v2");

			// Enable the application to use a cookie to store information for the signed in user
			// and to use a cookie to temporarily store information about a user logging in with a third party login provider
			// Configure the sign in cookie
			app.UseCookieAuthentication(new CookieAuthenticationOptions
			{
				TicketDataFormat = new AspNetTicketDataFormat(new DataProtectorShim(dataProtector)),
				AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
				AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Active,
				CookieDomain = "localhost",
				CookieName = ".AspNet.SharedCookie",
				CookiePath = "/",
				CookieSecure = CookieSecureOption.SameAsRequest
			});
		}
	}
}