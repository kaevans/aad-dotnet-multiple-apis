using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;

namespace aad_dotnet_multiple_apis.Models
{
    public class StorageModel
    {

        public StorageModel(IListBlobItem item)
        {
            this.Uri = item.Uri;
            this.Parent = item.Parent;
            this.Container = item.Container;
            this.StorageUri = item.StorageUri;
        }
        //
        // Summary:
        //     Gets the URI to the blob item, at the primary location.
        public Uri Uri { get; set; }
        //
        // Summary:
        //     Gets the blob item's URIs for both the primary and secondary locations.
        public StorageUri StorageUri { get; set; }
        //
        // Summary:
        //     Gets the blob item's parent virtual directory.
        public CloudBlobDirectory Parent { get; set; }
        //
        // Summary:
        //     Gets the blob item's container.
        public CloudBlobContainer Container { get; set; }
    }
}