using System.Diagnostics;

namespace CertificateManagerApp.Services;

public class CertificateManagementForMAUIService
{
    #region Windows
    public async Task GenerateCertificateForWindows(string publisherName, string password, string thumbprint, string filePath, IProgress<string> progress, CancellationToken cancellationToken)
    {
        // Crear certificado autofirmado
        var result = await ExecutePowerShellCommand($"New-SelfSignedCertificate -Type Custom -Subject 'CN={publisherName}' -KeyUsage DigitalSignature -FriendlyName 'My temp dev cert' -CertStoreLocation 'Cert:\\CurrentUser\\My' -TextExtension @('2.5.29.37={{text}}1.3.6.1.5.5.7.3.3', '2.5.29.19={{text}}')", progress, cancellationToken);

        if (result.Contains("Error")) return;

        // Convertir contraseña a formato seguro
        result = await ExecutePowerShellCommand($"$password = ConvertTo-SecureString -String {password} -Force -AsPlainText", progress, cancellationToken);

        if (result.Contains("Error")) return;

        // Exportar el certificado a un archivo PFX
        result = await ExecutePowerShellCommand($"Export-PfxCertificate -cert \"Cert:\\CurrentUser\\My\\{thumbprint}\" -FilePath {filePath}.pfx -Password $password", progress, cancellationToken);
    }

    async Task<string> ExecutePowerShellCommand(string command, IProgress<string> progress, CancellationToken cancellationToken)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-Command \"{command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        var tcs = new TaskCompletionSource<string>();

        process.OutputDataReceived += (sender, args) =>
        {
            if (args.Data != null)
            {
                progress.Report(args.Data);
                tcs.TrySetResult(args.Data);
            }
        };

        process.ErrorDataReceived += (sender, args) =>
        {
            if (args.Data != null)
            {
                progress.Report($"Error: {args.Data}");
                tcs.TrySetResult($"Error: {args.Data}");
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync(cancellationToken);

        return await tcs.Task;
    }
    #endregion
}
