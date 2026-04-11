using Traceon.Application.Common;
using Traceon.Contracts.ReceiptScan;

namespace Traceon.Application.Interfaces;

public interface IReceiptOcrService
{
    Task<Result<ReceiptScanResponse>> ScanReceiptAsync(Stream imageStream, string fileName, CancellationToken cancellationToken = default);
}
