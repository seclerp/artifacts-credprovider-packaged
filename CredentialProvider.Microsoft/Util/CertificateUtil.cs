﻿using System;
using System.Security.Cryptography.X509Certificates;
using ILogger = NuGetCredentialProvider.Logging.ILogger;

namespace NuGetCredentialProvider.Util;

public static class CertificateUtil
{
    public static X509Certificate2 GetCertificateBySubjectName(ILogger logger, string subjectName)
    {
        if (string.IsNullOrWhiteSpace(subjectName))
        {
            logger.Info(message: Resources.InvalidCertificateInput);
            return null;
        }

        var locations = new []{ StoreLocation.CurrentUser, StoreLocation.LocalMachine };
        foreach (var location in locations)
        {
            var store = new X509Store(StoreName.My, location);
            try
            {
                store.Open(OpenFlags.ReadOnly);
                var cert = store.Certificates.Find(X509FindType.FindBySubjectDistinguishedName , subjectName, false);

                if (cert.Count > 0)
                {
                    logger.Verbose(string.Format(Resources.ClientCertificateFound, subjectName));
                    return cert[0];
                }
            }
            catch (Exception ex)
            {
                logger.Error(string.Format(Resources.ClientCertificateError, ex, ex.Message));
                continue;
            }
            finally
            {
                store.Close();
            }
        }

        logger.Info(string.Format(Resources.ClientCertificateSubjectNameNotFound, subjectName));
        return null;
    }

    public static X509Certificate2 GetCertificateByFilePath(ILogger logger, string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            logger.Info(message: Resources.InvalidCertificateInput);
            return null;
        }

        try
        {
            var certificate = new X509Certificate2(filePath);

            if (certificate == null)
            {
                logger.Verbose(string.Format(Resources.ClientCertificateFilePathNotFound, filePath));
                return null;
            }

            logger.Verbose(string.Format(Resources.ClientCertificateFound, filePath));
            return certificate;
        }
        catch (Exception ex)
        {
            logger.Error(string.Format(Resources.ClientCertificateError, ex, ex.Message));
            return null;
        }
    }
}
