import type { ReactNode } from 'react'
import * as Dialog from '@radix-ui/react-dialog'

export function ConfirmDialog({
  title,
  description,
  confirmLabel = 'Ya, lanjutkan',
  cancelLabel = 'Batal',
  tone = 'danger',
  children,
  onConfirm,
}: {
  title: string
  description?: string
  confirmLabel?: string
  cancelLabel?: string
  tone?: 'danger' | 'primary'
  children: ReactNode
  onConfirm: () => void
}) {
  const buttonClass = tone === 'danger'
    ? 'bg-red-600 hover:bg-red-700'
    : 'bg-indigo-600 hover:bg-indigo-700'

  return (
    <Dialog.Root>
      <Dialog.Trigger asChild>{children}</Dialog.Trigger>
      <Dialog.Portal>
        <Dialog.Overlay className="fixed inset-0 z-50 bg-slate-950/50" />
        <Dialog.Content className="fixed left-1/2 top-1/2 z-50 w-[calc(100vw-2rem)] max-w-md -translate-x-1/2 -translate-y-1/2 rounded-2xl bg-white p-6 shadow-2xl">
          <Dialog.Title className="text-xl font-bold text-slate-900">{title}</Dialog.Title>
          {description && <Dialog.Description className="mt-2 text-sm leading-6 text-slate-500">{description}</Dialog.Description>}
          <div className="mt-6 flex justify-end gap-3">
            <Dialog.Close className="rounded-xl border border-slate-200 px-4 py-2 text-sm font-semibold text-slate-700 hover:bg-slate-50">{cancelLabel}</Dialog.Close>
            <Dialog.Close className={`rounded-xl px-4 py-2 text-sm font-semibold text-white ${buttonClass}`} onClick={onConfirm}>{confirmLabel}</Dialog.Close>
          </div>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  )
}
