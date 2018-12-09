using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;

namespace aad_dotnet_multiple_apis.Models
{
    public class StorageModel
    {

        public StorageModel(CloudBlockBlob item)
        {
            this.Uri = item.Uri;
            this.Parent = item.Parent;
            this.Container = item.Container;
            this.StorageUri = item.StorageUri;
            this.Name = item.Name;
            this.Created = item.Properties.Created;
            this.ETag = item.Properties.ETag;
            this.IsServerEncrypted = item.Properties.IsServerEncrypted;
            this.LastModified = item.Properties.LastModified;
            this.Length = item.Properties.Length;            
            
        }

        public StorageModel() { }

        //
        // Summary:
        //     Gets the URI to the blob item, at the primary location.
        public Uri Uri { get; set; }

        //
        // Summary:
        //     Gets the Name of the blob item
        public string Name { get; set; }

        public DateTimeOffset? Created { get; set; }

        public long Length { get; set; }

        public DateTimeOffset? LastModified { get; set; }


        public bool IsServerEncrypted { get; set; }

        public string ETag { get; set; }


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

        //
        // Summary:
        //     Gets the blob item's contents as a string.
        public string Contents { get; set; }
    }
}