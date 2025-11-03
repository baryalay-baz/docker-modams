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

        public List<vwTransfer> OutgoingTransfers = new List<vwTransfer>();
        public List<vwTransfer> IncomingTransfers = new List<vwTransfer>();

        public List<TransferChartDTO> IncomingChartData = new List<TransferChartDTO>();
        public List<TransferChartDTO> OutgoingChartData = new List<TransferChartDTO>();
        public List<TransferAssetDTO> TransferredAssets = new List<TransferAssetDTO>();

        [ValidateNever]
        public IEnumerable<SelectListItem> StoreList { get; set; }
        [ValidateNever]
        public string SelectedStoreName { get; set; } = string.Empty;

        public int TotalTransferCount(int storeId)
        {
            var outgoingTransfers = OutgoingTransfers.Where(m => m.TransferStatusId == 3 && m.StoreFromId == storeId).ToList();
            var totalAssets = outgoingTransfers.Sum(m => m.NumberOfAssets);

            return totalAssets;
        }
        public int TotalReceivedCount(int storeId)
        {
            var incomingTransfers = IncomingTransfers.Where(m => m.TransferStatusId == 3 && m.StoreId == storeId).ToList();
            var totalAssets = incomingTransfers.Sum(m => m.NumberOfAssets);

            return totalAssets;
        }
        public int GetAssetCount(int transferId) {
            return TransferredAssets.Where(m => m.TransferId == transferId).Count();
        }
        public string GetAssetsBadges(int transferId, int maxToShow = 3)
        {
            var items = TransferredAssets
                .Where(m => m.TransferId == transferId)
                .Select(m => new { m.AssetId, m.SerialNumber })
                .ToList();

            if (items.Count == 0) return string.Empty;

            // Normalize maxToShow
            if (maxToShow < 0) maxToShow = 0;
            if (maxToShow > items.Count) maxToShow = items.Count;

            var sb = new StringBuilder(items.Count * 80);

            // Render first N as clickable badges
            foreach (var it in items.Take(maxToShow))
            {
                var label = string.IsNullOrWhiteSpace(it.SerialNumber)
                    ? $"#{it.AssetId}"
                    : it.SerialNumber;

                var safeLabel = WebUtility.HtmlEncode(label);
                var href = $"/Users/Assets/AssetInfo/{it.AssetId}";
                var titleText = WebUtility.HtmlEncode($"View asset {it.AssetId}");

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

            // If there are more, add a visible "+N more" badge with tooltip of the rest
            int remaining = items.Count - maxToShow;
            if (remaining > 0)
            {
                var restLabels = items.Skip(maxToShow)
                    .Select(it => string.IsNullOrWhiteSpace(it.SerialNumber) ? $"#{it.AssetId}" : it.SerialNumber)
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


        public decimal TotalTransferValue { get; set; }
        public decimal TotalReceivedValue { get; set; }

    }

}
