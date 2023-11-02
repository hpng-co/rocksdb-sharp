using System;

namespace RocksDbSharp
{
    public class OptimisticTransactionOptions
    {
        public OptimisticTransactionOptions()
        {
            Handle = Native.Instance.rocksdb_optimistictransaction_options_create();
        }

        public IntPtr Handle { get; private set; }

        ~OptimisticTransactionOptions()
        {
            if (Handle == IntPtr.Zero)
                return;

#if !NODESTROY
            Native.Instance.rocksdb_optimistictransaction_options_destroy(Handle);
#endif
            Handle = IntPtr.Zero;
        }

        /// <summary>
        /// Setting the setSnapshot to true is the same as calling Transaction.SetSnapshot().
        /// Default: false
        /// </summary>
        /// <param name="value">Whether to set a snapshot</param>
        /// <returns></returns>
        public OptimisticTransactionOptions SetSetSnapshot(bool value)
        {
            Native.Instance.rocksdb_optimistictransaction_options_set_set_snapshot(Handle, value);
            return this;
        }
    }
}