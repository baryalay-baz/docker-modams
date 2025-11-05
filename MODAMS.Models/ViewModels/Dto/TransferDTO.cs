using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;
using System.Text;

namespace MODAMS.Models.ViewModels.Dto
{
    public class TransferDTO
    {
        public int StoreId { get; set; }
        public bool IsAuthorized { get; set; }

        [ValidateNever]
        public int TransferStatus { get; set; }

        // ------------ Data sets populated by the service ------------
        public List<vwTransfer> OutgoingTransfers { get; set; } = new();   // current store is StoreFromId
        public List<vwTransfer> IncomingTransfers { get; set; } = new();   // current store is StoreId
        public List<TransferChartDTO> IncomingChartData { get; set; } = new();
        public List<TransferChartDTO> OutgoingChartData { get; set; } = new();
        public List<TransferAssetDTO> TransferredAssets { get; set; } = new();

        [ValidateNever] public IEnumerable<SelectListItem> StoreList { get; set; } = Enumerable.Empty<SelectListItem>();
        [ValidateNever] public string SelectedStoreName { get; set; } = string.Empty;

        public decimal TotalTransferValue { get; set; }
        public decimal TotalReceivedValue { get; set; }

        // ------------ Action Center metrics ------------
        public int PendingAckOut { get; set; }
        public int PendingAckIn { get; set; }
        public int RejectedOut { get; set; }
        public int RejectedIn { get; set; }
        public int OverdueOut { get; set; }   // Awaiting Ack & older than N days
        public int OverdueIn { get; set; }

        public int TotalPendingAck => PendingAckOut + PendingAckIn;
        public int TotalRejected => RejectedOut + RejectedIn;
        public int TotalOverdue => OverdueOut + OverdueIn;

        public int OverdueDaysThreshold { get; set; } = 7;

        // ------------ KPIs by Store ------------
        // Default statusId = 3 (Completed). Pass 0 to ignore status filter.
        public int GetTransferCountOut(int storeId, int statusId = 3)
        {
            var q = OutgoingTransfers.Where(t => t.StoreFromId == storeId);
            if (statusId > 0) q = q.Where(t => t.TransferStatusId == statusId);
            return q.Count();
        }

        public int GetTransferCountIn(int storeId, int statusId = 3)
        {
            var q = IncomingTransfers.Where(t => t.StoreId == storeId);
            if (statusId > 0) q = q.Where(t => t.TransferStatusId == statusId);
            return q.Count();
        }

        public int GetAssetCountOut(int storeId, int statusId = 3)
        {
            var transferIds = OutgoingTransfers
                .Where(t => t.StoreFromId == storeId && (statusId == 0 || t.TransferStatusId == statusId))
                .Select(t => t.Id)
                .ToHashSet();

            if (transferIds.Count == 0) return 0;
            return TransferredAssets.Count(a => transferIds.Contains(a.TransferId));
        }

        public int GetAssetCountIn(int storeId, int statusId = 3)
        {
            var transferIds = IncomingTransfers
                .Where(t => t.StoreId == storeId && (statusId == 0 || t.TransferStatusId == statusId))
                .Select(t => t.Id)
                .ToHashSet();

            if (transferIds.Count == 0) return 0;
            return TransferredAssets.Count(a => transferIds.Contains(a.TransferId));
        }

        public string GetAssetsBadges(int transferId, int maxToShow = 3)
        {
            var items = TransferredAssets
                .Where(m => m.TransferId == transferId)
                .Select(m => new { m.AssetId, m.SerialNumber, m.AssetName })
                .ToList();

            if (items.Count == 0) return string.Empty;

            if (maxToShow < 0) maxToShow = 0;
            if (maxToShow > items.Count) maxToShow = items.Count;

            var sb = new StringBuilder(items.Count * 80);

            foreach (var it in items.Take(maxToShow))
            {
                var label = string.IsNullOrWhiteSpace(it.SerialNumber) ? $"#{it.AssetId}" : it.SerialNumber;
                var safeLabel = WebUtility.HtmlEncode(label);
                var href = $"/Users/Assets/AssetInfo/{it.AssetId}";
                var titleText = WebUtility.HtmlEncode($"View Details: {it.AssetName}");

                sb.Append("<a href=\"")
                  .Append(href)
                  .Append("\" class=\"badge asset-badge rounded-pill me-1 mb-1\" ")
                  .Append("data-asset-badge ")
                  .Append("data-bs-toggle=\"tooltip\" data-bs-placement=\"top\" ")
                  .Append("title=\"").Append(titleText).Append("\" ")
                  .Append("aria-label=\"").Append(titleText).Append("\">")
                  .Append(safeLabel)
                  .Append("</a>");
            }

            int remaining = items.Count - maxToShow;
            if (remaining > 0)
            {
                var restLabels = items.Skip(maxToShow)
                    .Select(it => it.AssetName) //string.IsNullOrWhiteSpace(it.SerialNumber) ? $"#{it.AssetId}" : it.SerialNumber)
                    .ToList();

                var tooltip = WebUtility.HtmlEncode(string.Join(", ", restLabels));
                var aria = WebUtility.HtmlEncode($"+{remaining} more assets");

                sb.Append("<span class=\"badge badge-more badge-more--emphasis me-1 mb-1\" ")
                  .Append("role=\"button\" tabindex=\"0\" ")
                  .Append("data-bs-toggle=\"tooltip\" data-bs-placement=\"top\" ")
                  .Append("data-bs-custom-class=\"tooltip-lg\" ")
                  .Append("title=\"").Append(tooltip).Append("\" ")
                  .Append("aria-label=\"").Append(aria).Append("\">")
                  .Append("&#43;").Append(remaining).Append("&nbsp;more")
                  .Append("</span>");
            }

            return sb.ToString();
        }
    }
}
