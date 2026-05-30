import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { DataTable, type Column, type PageResponse } from '../components/DataTable'
import { api } from '../lib/api'

type Props<T extends Record<string, unknown>> = { title: string; endpoint: string; columns: Column<T>[] }

export function ListPage<T extends Record<string, unknown>>({ title, endpoint, columns }: Props<T>) {
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const [sortBy, setSortBy] = useState<string | undefined>('code')
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('asc')
  const { data, isLoading } = useQuery({
    queryKey: [endpoint, page, search, sortBy, sortDirection],
    queryFn: async () => (await api.get<PageResponse<T>>(endpoint, { params: { page, pageSize: 20, search, sortBy, sortDirection } })).data,
  })
  return <DataTable title={title} columns={columns} data={data} isLoading={isLoading} search={search} sortBy={sortBy} sortDirection={sortDirection} onSearch={(value) => { setSearch(value); setPage(1) }} onPage={setPage} onSort={(key) => { setSortBy(key); setSortDirection(sortBy === key && sortDirection === 'asc' ? 'desc' : 'asc') }} actions={<button className="rounded-xl bg-indigo-600 px-4 py-2 font-semibold text-white hover:bg-indigo-700">Tambah</button>} />
}

export const itemColumns: Column<Record<string, unknown>>[] = [
  { key: 'sku', header: 'SKU', sortable: true },
  { key: 'name', header: 'Nama', sortable: true },
  { key: 'barcode', header: 'Barcode', sortable: true },
  { key: 'baseUnit', header: 'Unit', sortable: true },
  { key: 'isActive', header: 'Aktif', render: (row) => row.isActive ? 'Ya' : 'Tidak' },
]

export const customerColumns: Column<Record<string, unknown>>[] = [
  { key: 'code', header: 'Kode', sortable: true },
  { key: 'name', header: 'Nama', sortable: true },
  { key: 'phone', header: 'Phone', sortable: true },
  { key: 'email', header: 'Email', sortable: true },
  { key: 'currentDebt', header: 'Piutang' },
  { key: 'isActive', header: 'Aktif', render: (row) => row.isActive ? 'Ya' : 'Tidak' },
]

export const supplierColumns: Column<Record<string, unknown>>[] = [
  { key: 'code', header: 'Kode', sortable: true },
  { key: 'name', header: 'Nama', sortable: true },
  { key: 'phone', header: 'Phone', sortable: true },
  { key: 'email', header: 'Email', sortable: true },
  { key: 'isActive', header: 'Aktif', render: (row) => row.isActive ? 'Ya' : 'Tidak' },
]

export const branchColumns: Column<Record<string, unknown>>[] = [
  { key: 'code', header: 'Kode', sortable: true },
  { key: 'name', header: 'Nama', sortable: true },
  { key: 'phone', header: 'Phone', sortable: true },
  { key: 'address', header: 'Alamat', sortable: true },
  { key: 'isActive', header: 'Aktif', render: (row) => row.isActive ? 'Ya' : 'Tidak' },
]
