import { ArrowDown, ArrowUp, Search } from 'lucide-react'

export type PageResponse<T> = { items: T[]; page: number; pageSize: number; totalItems: number; totalPages: number }
export type Column<T> = { key: keyof T | string; header: string; sortable?: boolean; render?: (row: T) => React.ReactNode }

type Props<T> = {
  title: string
  columns: Column<T>[]
  data?: PageResponse<T>
  isLoading?: boolean
  search: string
  sortBy?: string
  sortDirection?: 'asc' | 'desc'
  onSearch: (value: string) => void
  onSort: (key: string) => void
  onPage: (page: number) => void
  actions?: React.ReactNode
}

export function DataTable<T extends Record<string, unknown>>({ title, columns, data, isLoading, search, sortBy, sortDirection, onSearch, onSort, onPage, actions }: Props<T>) {
  return (
    <div className="rounded-2xl border border-slate-200 bg-white shadow-sm">
      <div className="flex flex-col gap-4 border-b border-slate-200 p-5 md:flex-row md:items-center md:justify-between">
        <div>
          <h2 className="text-xl font-bold text-slate-900">{title}</h2>
          <p className="text-sm text-slate-500">Server paging, search, filter, sort.</p>
        </div>
        <div className="flex gap-3">
          <label className="relative">
            <Search className="absolute left-3 top-2.5 text-slate-400" size={18} />
            <input value={search} onChange={(e) => onSearch(e.target.value)} className="w-64 rounded-xl border border-slate-300 py-2 pl-10 pr-3 outline-none focus:border-indigo-500" placeholder="Cari data..." />
          </label>
          {actions}
        </div>
      </div>
      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-slate-200 text-sm">
          <thead className="bg-slate-50">
            <tr>{columns.map((column) => <th key={String(column.key)} className="px-5 py-3 text-left font-semibold text-slate-600"><button className="inline-flex items-center gap-1" onClick={() => column.sortable && onSort(String(column.key))}>{column.header}{sortBy === column.key && (sortDirection === 'desc' ? <ArrowDown size={14} /> : <ArrowUp size={14} />)}</button></th>)}</tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {isLoading ? <tr><td className="px-5 py-8 text-center text-slate-500" colSpan={columns.length}>Loading...</td></tr> : data?.items?.map((row, index) => <tr key={String(row.id ?? index)} className="hover:bg-slate-50">{columns.map((column) => <td key={String(column.key)} className="px-5 py-3 text-slate-700">{column.render ? column.render(row) : String(row[column.key] ?? '')}</td>)}</tr>)}
            {!isLoading && data?.items?.length === 0 && <tr><td className="px-5 py-8 text-center text-slate-500" colSpan={columns.length}>Data kosong</td></tr>}
          </tbody>
        </table>
      </div>
      <div className="flex items-center justify-between border-t border-slate-200 p-5 text-sm text-slate-600">
        <span>Total {data?.totalItems ?? 0} data</span>
        <div className="flex items-center gap-2">
          <button className="rounded-lg border px-3 py-2 disabled:opacity-50" disabled={!data || data.page <= 1} onClick={() => onPage((data?.page ?? 1) - 1)}>Prev</button>
          <span>Page {data?.page ?? 1} / {data?.totalPages ?? 1}</span>
          <button className="rounded-lg border px-3 py-2 disabled:opacity-50" disabled={!data || data.page >= data.totalPages} onClick={() => onPage((data?.page ?? 1) + 1)}>Next</button>
        </div>
      </div>
    </div>
  )
}
