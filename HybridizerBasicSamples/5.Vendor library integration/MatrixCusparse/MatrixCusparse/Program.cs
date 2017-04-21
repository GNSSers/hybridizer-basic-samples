﻿using Hybridizer.Runtime.CUDAImports;
using Hybridizer.Runtime.CUDAImports.cusparse;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MatrixCusparse
{
    class Program
    {
        static void Main(string[] args)
        {
            const int redo = 30;

            const int rowsCount = 20000000;
            SparseMatrix a = SparseMatrix.Laplacian_1D(rowsCount);
            
            float[] x = VectorReader.GetSplatVector(a.rows.Length - 1, 1.0F);
            float[] b = new float[x.Length];
               
            Stopwatch watch = new Stopwatch();

            float alpha = 1.0f;
            float beta = 0.0f;

            cusparseHandle_t handle;
            CUBSPARSE_64_75.cusparseCreate(out handle);

            cusparseOperation_t transA = cusparseOperation_t.CUSPARSE_OPERATION_NON_TRANSPOSE;

            cusparseMatDescr_t descrA;
            CUBSPARSE_64_75.cusparseCreateMatDescr(out descrA);
            CUBSPARSE_64_75.cusparseSetMatType(descrA, cusparseMatrixType_t.CUSPARSE_MATRIX_TYPE_GENERAL);
            CUBSPARSE_64_75.cusparseSetMatIndexBase(descrA , cusparseIndexBase_t.CUSPARSE_INDEX_BASE_ZERO);
            
            watch.Start();

            for (int i = 0; i < redo; ++i)
            {
               Multiply(handle, transA, a.rows.Length -1, x.Length,a.data.Length,alpha, descrA, a.data,a.rows,a.indices,x,beta,b);
            }

            watch.Stop();
            CUBSPARSE_64_75.cusparseDestroy(handle);

            Console.Out.WriteLine("DONE");

        }

        [IntrinsicFunction("fprintf")]
        public static void fprintf(string s)
        {
            Console.WriteLine(s);
        }
        
        public static unsafe void Multiply(cusparseHandle_t handle,
                                     cusparseOperation_t transA,
                                     int m,
                                     int n,
                                     int nnz,
                                     float alpha,
                                     cusparseMatDescr_t descrA,
                                     float[] csrValA,
                                     int[] csrRowPtrA,
                                     int[] csrColIndA,
                                     float[] x,
                                     float beta,
                                     float[] b
                                     )
        {
            cusparseScsrmv(handle, transA, m, n, nnz, &alpha, descrA, csrValA, csrRowPtrA, csrColIndA, x, &beta, b);
        }
        
        [DllImport("cusparse64_75.dll", EntryPoint = "cusparseScsrmv", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe cusparseStatus_t cusparseScsrmv(cusparseHandle_t handle,
                                              cusparseOperation_t transA,
                                              int m,
                                              int n,
                                              int nnz,
                                              float* alpha,
                                              cusparseMatDescr_t descrA,
                                              [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(CudaMarshaler))] float[] csrValA,
                                              [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(CudaMarshaler))] int[] csrRowPtrA,
                                              [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(CudaMarshaler))] int[] csrColIndA,
                                              [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(CudaMarshaler))] float[] x,
                                              float* beta,
                                              [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(CudaMarshaler))] float[] b);
    }
}