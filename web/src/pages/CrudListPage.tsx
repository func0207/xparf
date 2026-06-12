import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { DataTable, type Column, type PageResponse } from '../components/DataTable'
import { api, getApiErrorMessage } from '../lib/api'

export type FieldKind = 'text' | 'number' | 'checkbox' | 'select'
export type FormField<T extends Record<string, unknown>> = {
  key: keyof T & string
  label: string
  kind?: FieldKind
  required?: boolean
  options?: { label: string; value: string | number | boolean }[]
}

type Props<T extends Record<string, unknown>> = {
  title: string
  endpoint: string
  columns: Column<T>[]
  fields: FormField<T>[]
  createDefaults: T
  updateShape?: (value: T) => Record<string, unknown>
  sortDefault?: string
}

export function CrudListPage<T extends Record<string, unknown>>({ title, endpoint, columns, fields, createDefaults, updateShape, sortDefault = 'code' }: Props<T>) {
  const queryClient = useQueryClient()
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const [sortBy, setSortBy] = useState<string | undefined>(sortDefault)
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('asc')
  const [editing, setEditing] = useState<T | null>(null)
  const [form, setForm] = useState<T>(createDefaults)
  const [isFormOpen, setIsFormOpen] = useState(false)

  const queryKey = [endpoint, page, search, sortBy, sortDirection]
  const { data, isLoading } = useQuery({
    queryKey,
    queryFn: async () => (await api.get<PageResponse<T>>(endpoint, { params: { page, pageSize: 20, search, sortBy, sortDirection } })).data,
  })

  const saveMutation = useMutation({
    mutationFn: async (value: T) => {
      if (editing?.id) return api.put(`${endpoint}/${editing.id}`, updateShape ? updateShape(value) : value)
      return api.post(endpoint, value)
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [endpoint] })
      closeForm()
    },
  })

  const deleteMutation = useMutation({
    mutationFn: async (row: T) => api.delete(`${endpoint}/${row.id}`),
    onSuccess: async () => queryClient.invalidateQueries({ queryKey: [endpoint] }),
  })

  function openCreate() {
    setEditing(null)
    setForm({ ...createDefaults })
    setIsFormOpen(true)
  }

  function openEdit(row: T) {
    setEditing(row)
    setForm({ ...createDefaults, ...row })
    setIsFormOpen(true)
  }

  function closeForm() {
    setEditing(null)
    setForm({ ...createDefaults })
    setIsFormOpen(false)
  }

  const actionColumn: Column<T> = {
    key: '__actions',
    header: 'Aksi',
    render: (row) => (
      <div className="flex gap-2">
        <button className="rounded-lg border px-3 py-1 text-xs font-semibold hover:bg-slate-100" onClick={() => openEdit(row)}>Edit</button>
        <button className="rounded-lg border border-red-200 px-3 py-1 text-xs font-semibold text-red-600 hover:bg-red-50" onClick={() => confirm('Hapus data ini?') && deleteMutation.mutate(row)}>Hapus</button>
      </div>
    ),
  }

  return (
    <>
      <DataTable
        title={title}
        columns={[...columns, actionColumn]}
        data={data}
        isLoading={isLoading}
        search={search}
        sortBy={sortBy}
        sortDirection={sortDirection}
        onSearch={(value) => { setSearch(value); setPage(1) }}
        onPage={setPage}
        onSort={(key) => { setSortBy(key); setSortDirection(sortBy === key && sortDirection === 'asc' ? 'desc' : 'asc') }}
        actions={<button className="rounded-xl bg-indigo-600 px-4 py-2 font-semibold text-white hover:bg-indigo-700" onClick={openCreate}>Tambah</button>}
      />
      {isFormOpen && (
        <FormPanel
          title={editing ? `Edit ${title}` : `Tambah ${title}`}
          fields={fields}
          value={form}
          isSaving={saveMutation.isPending}
          error={saveMutation.error ? getApiErrorMessage(saveMutation.error, 'Gagal menyimpan data. Cek validasi/API.') : undefined}
          onClose={closeForm}
          onChange={setForm}
          onSubmit={() => saveMutation.mutate(form)}
        />
      )}
    </>
  )
}

type FormPanelProps<T extends Record<string, unknown>> = {
  title: string
  fields: FormField<T>[]
  value: T
  isSaving: boolean
  error?: string
  onClose: () => void
  onChange: (value: T) => void
  onSubmit: () => void
}

function FormPanel<T extends Record<string, unknown>>({ title, fields, value, isSaving, error, onClose, onChange, onSubmit }: FormPanelProps<T>) {
  return (
    <div className="fixed inset-0 z-50 flex justify-end bg-slate-900/30">
      <div className="h-full w-full max-w-xl overflow-y-auto bg-white p-6 shadow-2xl">
        <div className="flex items-center justify-between border-b border-slate-200 pb-4">
          <h3 className="text-xl font-bold">{title}</h3>
          <button className="rounded-lg border px-3 py-2 text-sm" onClick={onClose}>Tutup</button>
        </div>
        <div className="mt-5 space-y-4">
          {fields.map((field) => (
            <label key={field.key} className="block">
              <span className="text-sm font-medium text-slate-700">{field.label}</span>
              {field.kind === 'checkbox' ? (
                <input className="mt-2 block h-5 w-5" type="checkbox" checked={Boolean(value[field.key])} onChange={(e) => onChange({ ...value, [field.key]: e.target.checked })} />
              ) : field.kind === 'select' ? (
                <select className="mt-1 w-full rounded-xl border border-slate-300 px-4 py-3 outline-none focus:border-indigo-500" value={String(value[field.key] ?? '')} onChange={(e) => onChange({ ...value, [field.key]: coerceSelectValue(e.target.value, field.options) })}>
                  {field.options?.map((option) => <option key={String(option.value)} value={String(option.value)}>{option.label}</option>)}
                </select>
              ) : (
                <input className="mt-1 w-full rounded-xl border border-slate-300 px-4 py-3 outline-none focus:border-indigo-500" type={field.kind === 'number' ? 'number' : 'text'} required={field.required} value={String(value[field.key] ?? '')} onChange={(e) => onChange({ ...value, [field.key]: field.kind === 'number' ? Number(e.target.value) : e.target.value })} />
              )}
            </label>
          ))}
          {error && <p className="rounded-xl bg-red-50 p-3 text-sm text-red-700">{error}</p>}
          <button disabled={isSaving} className="w-full rounded-xl bg-indigo-600 px-4 py-3 font-semibold text-white hover:bg-indigo-700 disabled:opacity-60" onClick={onSubmit}>{isSaving ? 'Menyimpan...' : 'Simpan'}</button>
        </div>
      </div>
    </div>
  )
}

function coerceSelectValue(value: string, options?: { value: string | number | boolean }[]) {
  const option = options?.find((item) => String(item.value) === value)
  return option?.value ?? value
}
