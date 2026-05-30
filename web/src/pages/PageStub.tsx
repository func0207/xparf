type PageStubProps = { title: string; description: string }

export function PageStub({ title, description }: PageStubProps) {
  return (
    <div className="rounded-2xl border border-dashed border-slate-300 bg-white p-8">
      <h2 className="text-3xl font-bold text-slate-900">{title}</h2>
      <p className="mt-2 text-slate-500">{description}</p>
      <p className="mt-6 rounded-xl bg-amber-50 p-4 text-sm text-amber-800">
        Placeholder siap. Next chunk: sambungkan endpoint API, DataTable, FormModal, validasi zod.
      </p>
    </div>
  )
}
