using System;
using System.Collections.Generic;
using System.Linq;

namespace RocksDbSharp
{
    public sealed class OptimisticTransactionDb : RocksDbBase
    {
        internal static TransactionOptions DefaultTransactionOptions { get; set; } = new TransactionOptions();

        private OptimisticTransactionDb(IntPtr handle, dynamic optionsReferences, dynamic cfOptionsRefs, Dictionary<string, ColumnFamilyHandleInternal> columnFamilies = null)
            : base(handle, (object)optionsReferences, (object)cfOptionsRefs, columnFamilies)
        {
        }

        protected override void ReleaseUnmanagedResources()
        {
            base.ReleaseUnmanagedResources();

            if (Handle == IntPtr.Zero)
                return;

            var handle = Handle;
            Handle = IntPtr.Zero;
            Native.Instance.rocksdb_optimistictransactiondb_close(handle);
        }

        public static OptimisticTransactionDb Open(OptionsHandle options, string path)
        {
            using (var pathSafe = new RocksSafePath(path))
            {
                IntPtr db = Native.Instance.rocksdb_optimistictransactiondb_open(options.Handle, pathSafe.Handle);
                return new OptimisticTransactionDb(db, optionsReferences: options, cfOptionsRefs: null);
            }
        }

        public static OptimisticTransactionDb Open(DbOptions options, string path, ColumnFamilies columnFamilies)
        {
            using (var pathSafe = new RocksSafePath(path))
            {
                string[] cfnames = columnFamilies.Names.ToArray();
                IntPtr[] cfoptions = columnFamilies.OptionHandles.ToArray();
                IntPtr[] cfhandles = new IntPtr[cfnames.Length];
                IntPtr db = Native.Instance.rocksdb_optimistictransactiondb_open_column_families(options.Handle, pathSafe.Handle, cfnames.Length, cfnames, cfoptions, cfhandles);
                var cfHandleMap = new Dictionary<string, ColumnFamilyHandleInternal>();
                foreach (var pair in cfnames.Zip(cfhandles.Select(cfh => new ColumnFamilyHandleInternal(cfh)), (name, cfh) => new { Name = name, Handle = cfh }))
                {
                    cfHandleMap.Add(pair.Name, pair.Handle);
                }

                return new OptimisticTransactionDb(db,
                    optionsReferences: options.References,
                    cfOptionsRefs: columnFamilies.Select(cfd => cfd.Options.References).ToArray(),
                    columnFamilies: cfHandleMap);
            }
        }

        public OptimisticTransaction BeginTransaction(WriteOptions writeOptions = null, TransactionOptions transactionOptions = null)
        {
            return new OptimisticTransaction(this,
                writeOptions ?? DefaultWriteOptions,
                transactionOptions ?? DefaultTransactionOptions);
        }
    }
}