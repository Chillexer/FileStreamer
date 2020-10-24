using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using WebApi.Model;

namespace WebApi.Controllers
{
    [Route("/")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        [HttpGet("drives")]
        public ActionResult GetDrives()
        {
            return Ok(Directory.GetLogicalDrives());
        }

        [HttpGet("directories")]
        public ActionResult GetFiles([FromBody] string path, [FromQuery]bool Recursive)
        {
            if (string.IsNullOrWhiteSpace(path))
                return BadRequest("Path can't be null or empty");

            if (!IsValidPath(path))
                return BadRequest("Path is not correct format");

            if (!Directory.Exists(path))
                return NotFound("Directory not found");

            var response = GetFolderResponse(path);
            if (!Recursive)
                return Ok(response);

            foreach (var entry in response.Entries)
            {
                RecursivePopulateItems(entry);
            }

            return Ok(response);
        }

        private FolderResponse GetFolderResponse(string path)
        {
            var previousFolder = Directory.GetParent(path);
            var response = new FolderResponse()
            {
                Entries = GetEntriesFromPath(path),
                PreviousFolder = previousFolder != null ? previousFolder.FullName : ""
            };
            foreach (var entry in response.Entries)
            {
                PopulateEntry(entry);
            }

            return response;
        }

        private List<Entry> GetEntriesFromPath(string path)
        {
            return (from f in Directory.GetFileSystemEntries(path)
                    select new Entry()
                    {
                        Path = f,
                    }).ToList();
        }

        private void PopulateEntry(Entry entry)
        {
            try
            {
                var directoryInfo = new DirectoryInfo(entry.Path);


                entry.IsAFolder = Directory.Exists(entry.Path);
                entry.Extension = directoryInfo.Extension;

                if (directoryInfo.Attributes.HasFlag(FileAttributes.ReadOnly))
                {
                    entry.IsReadable = true;
                    entry.IsWriteable = false;
                }
                else
                {
                    entry.IsReadable = true;
                    entry.IsWriteable = true;
                }


                if (directoryInfo.Attributes.HasFlag(FileAttributes.Directory))
                {
                    entry.ItemsInFolder = directoryInfo.GetFileSystemInfos().ToList().Count;
                }
            }
            catch (Exception)
            {
                entry.IsReadable = false;
                entry.IsWriteable = false;
            }
        }

        private Entry PopulateItemsIfEntryIsFolder(Entry entry)
        {
            if (!entry.IsAFolder)
                return entry;

            entry.Items = GetEntriesFromPath(entry.Path);

            foreach(var item in entry.Items)
            {
                PopulateEntry(item);
            }
            return entry;
        }

        private Entry RecursivePopulateItems(Entry entry)
        {
            if (!entry.IsAFolder)
                return entry;
            PopulateItemsIfEntryIsFolder(entry);
            foreach (var item in entry.Items)
            {
                PopulateEntry(item);
                if (item.ItemsInFolder > 0)
                {
                    RecursivePopulateItems(item);
                }
                else continue;
            }
            return entry;

        }
        public static bool IsValidPath(string path)
        {
            string result;
            return TryGetFullPath(path, out result);
        }

        public static bool TryGetFullPath(string path, out string result)
        {
            result = String.Empty;
            if (String.IsNullOrWhiteSpace(path)) { return false; }
            bool status = false;

            try
            {
                result = Path.GetFullPath(path);
                status = true;
            }
            catch (ArgumentException) { }
            catch (SecurityException) { }
            catch (NotSupportedException) { }
            catch (PathTooLongException) { }

            return status;
        }
    }
}
