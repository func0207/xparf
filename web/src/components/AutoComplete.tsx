import { useMemo, useState } from 'react'

export type AutoCompleteOption<T extends string | number = string | number> = {
  value: T
  label: string
  description?: string
}

export function AutoComplete<T extends string | number>({
  label,
  value,
  options,
  placeholder = 'Cari...',
  onChange,
}: {
  label: string
  value: T | ''
  options: AutoCompleteOption<T>[]
  placeholder?: string
  onChange: (value: T | '') => void
}) {
  const selected = options.find((option) => option.value === value)
  const [query, setQuery] = useState(selected?.label ?? '')
  const [open, setOpen] = useState(false)

  const filtered = useMemo(() => {
    const needle = query.trim().toLowerCase()
    return options.filter((option) => !needle || `${option.label} ${option.description ?? ''}`.toLowerCase().includes(needle)).slice(0, 20)
  }, [options, query])

  return (
    <label className="relative block">
      <span className="text-sm font-medium text-slate-700">{label}</span>
      <input
        className="mt-1 w-full rounded-xl border border-slate-300 px-4 py-3 outline-none focus:border-indigo-500 focus:ring-4 focus:ring-indigo-100"
        value={query}
        placeholder={placeholder}
        onFocus={() => setOpen(true)}
        onChange={(event) => {
          setQuery(event.target.value)
          onChange('')
          setOpen(true)
        }}
      />
      {open && (
        <div className="absolute z-40 mt-2 max-h-72 w-full overflow-y-auto rounded-2xl border border-slate-200 bg-white p-1 shadow-xl">
          {filtered.map((option) => (
            <button
              key={String(option.value)}
              type="button"
              className="block w-full rounded-xl px-3 py-2 text-left hover:bg-indigo-50"
              onMouseDown={(event) => event.preventDefault()}
              onClick={() => {
                onChange(option.value)
                setQuery(option.label)
                setOpen(false)
              }}
            >
              <span className="block font-semibold text-slate-800">{option.label}</span>
              {option.description && <span className="block text-xs text-slate-500">{option.description}</span>}
            </button>
          ))}
          {filtered.length === 0 && <div className="px-3 py-4 text-sm text-slate-500">Tidak ada data</div>}
        </div>
      )}
    </label>
  )
}
