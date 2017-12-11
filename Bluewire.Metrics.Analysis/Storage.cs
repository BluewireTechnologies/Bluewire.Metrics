using System;
using System.Collections.Generic;
using System.IO;
using Sparrow;
using Sparrow.Json;
using Voron;
using Voron.Data.Tables;
using Voron.Impl;

namespace Bluewire.Metrics.Analysis
{
    public class Storage
    {
        private readonly StorageEnvironment environment;

        static Storage()
        {
            Slice.From(StorageEnvironment.LabelsContext, "AllDocuments", ByteStringType.Immutable, out AllDocumentsSlice);
            Slice.From(StorageEnvironment.LabelsContext, "LocalDocuments", ByteStringType.Immutable, out LocalDocumentsSlice);
            DocumentsSchema = new TableSchema() { TableType = 1 };
            DocumentsSchema.DefineKey(new TableSchema.SchemaIndexDef {
                IsGlobal = true,
                Name = AllDocumentsSlice,
                Count = 1,
                StartIndex = 0
            });
            DocumentByTimestampIndexDef = new TableSchema.FixedSizeSchemaIndexDef {
                IsGlobal = false,
                Name = LocalDocumentsSlice,
                StartIndex = 1
            };
            DocumentsSchema.DefineFixedSizeIndex(DocumentByTimestampIndexDef);
        }

        private static readonly TableSchema.FixedSizeSchemaIndexDef DocumentByTimestampIndexDef;

        public static readonly Slice LocalDocumentsSlice;
        public static readonly Slice AllDocumentsSlice;

        public static TableSchema DocumentsSchema { get; }

        public Storage(StorageEnvironment environment)
        {
            this.environment = environment;
        }

        public void AddDocumentsForSite(string siteId, IEnumerable<Document> documents)
        {
            using (var tx = environment.WriteTransaction())
            using (new JsonContextPool().AllocateOperationContext(out var jsonContext))
            {
                DocumentsSchema.Create(tx, siteId, null);

                var table = tx.OpenTable(DocumentsSchema, siteId);
                foreach (var doc in documents)
                {
                    var id = $"{siteId}/{doc.Id.Ticks}";

                    WriteDocument(tx, table, jsonContext, id, doc);
                }
                tx.Commit();
            }
        }

        private static unsafe void WriteDocument(Transaction tx, Table table, JsonOperationContext jsonContext, string id, Document document)
        {
            using (Slice.From(tx.Allocator, id, out var idSlice))
            using (var json = jsonContext.ReadForDisk(document.Stream, id))
            {
                table.ReadByKey(idSlice, out var reader);
                using (table.Allocate(out var builder))
                {
                    builder.Add(idSlice);
                    builder.Add(document.Id.UtcTicks);
                    builder.Add(document.Id.Offset.Ticks);
                    builder.Add(json.BasePointer, json.Size);
                    if ((IntPtr)reader.Pointer == IntPtr.Zero)
                    {
                        table.Insert(builder);
                    }
                    else
                    {
                        table.Update(reader.Id, builder, false);
                    }
                }
            }
        }

        public IEnumerable<Document> EnumerateDocumentsForSite(string siteId)
        {
            using (var tx = environment.ReadTransaction())
            using (new JsonContextPool().AllocateOperationContext(out var jsonContext))
            {
                var table = tx.OpenTable(DocumentsSchema, siteId);
                if (table == null) yield break;
                foreach (var value in table.SeekForwardFrom(DocumentByTimestampIndexDef, 0, 0))
                {
                    yield return ReadDocument(value, jsonContext);
                }
            }
        }

        private static unsafe Document ReadDocument(Table.TableValueHolder value, JsonOperationContext jsonContext)
        {
            var ticksPtr = (long*) value.Reader.Read(1, out _);
            var offsetPtr = (long*) value.Reader.Read(2, out _);
            var jsonPtr = value.Reader.Read(3, out var size);
            var json = new BlittableJsonReaderObject(jsonPtr, size, jsonContext, new UnmanagedWriteBuffer());
            var ms = new MemoryStream();
            json.WriteJsonTo(ms);
            return new Document {
                Id = new DateTimeOffset(*ticksPtr, new TimeSpan(*offsetPtr)),
                Stream = ms
            };
        }
    }
}
