import type { InputHTMLAttributes, ReactNode } from 'react'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Link, useNavigate } from 'react-router-dom'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { api, getApiErrorMessage, type AuthResponse } from '../lib/api'
import { useAuthStore } from '../store/auth-store'

const schema = z.object({ email: z.string().email('Email tidak valid'), password: z.string().min(8, 'Password minimal 8 karakter') })
type FormValues = z.infer<typeof schema>

export function LoginPage() {
  const navigate = useNavigate()
  const setAuth = useAuthStore((state) => state.setAuth)
  const [error, setError] = useState('')
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<FormValues>({ resolver: zodResolver(schema) })

  const onSubmit = async (values: FormValues) => {
    setError('')
    try {
      const payload = { email: values.email.trim().toLowerCase(), password: values.password }
      const { data } = await api.post<AuthResponse>('/auth/login', payload)
      setAuth(data)
      navigate('/dashboard', { replace: true })
    } catch (err) {
      setError(getApiErrorMessage(err, 'Login gagal. Cek email/password atau status akun.'))
    }
  }

  return (
    <AuthShell title="Login XPARF" subtitle="Masuk ke dashboard ERP" sideTitle="Selamat datang kembali" sideText="Lanjutkan testing flow master data, stok, transaksi, billing, report, dan struk thermal.">
      <form className="space-y-4" onSubmit={handleSubmit(onSubmit)}>
        {error && <p className="rounded-xl bg-red-50 p-3 text-sm font-medium text-red-700">{error}</p>}
        <Input label="Email" type="email" placeholder="owner@company.com" autoComplete="email" error={errors.email?.message} {...register('email')} />
        <Input label="Password" type="password" placeholder="Minimal 8 karakter" autoComplete="current-password" error={errors.password?.message} {...register('password')} />
        <div className="flex justify-end"><Link className="text-sm font-semibold text-indigo-600" to="/forgot-password">Lupa password?</Link></div>
        <button disabled={isSubmitting} className="w-full rounded-xl bg-indigo-600 px-4 py-3 font-semibold text-white hover:bg-indigo-700 disabled:opacity-60">{isSubmitting ? 'Masuk...' : 'Login'}</button>
        <p className="text-center text-sm text-slate-500">Belum punya akun? <Link className="font-semibold text-indigo-600" to="/register">Register company</Link></p>
      </form>
    </AuthShell>
  )
}

export function AuthShell({ title, subtitle, sideTitle, sideText, children }: { title: string; subtitle: string; sideTitle?: string; sideText?: string; children: ReactNode }) {
  return <div className="grid min-h-screen bg-slate-950 text-slate-900 lg:grid-cols-2">
    <section className="relative hidden overflow-hidden p-10 text-white lg:block"><div className="absolute inset-0 bg-[radial-gradient(circle_at_top,_rgba(99,102,241,0.45),transparent_35%),radial-gradient(circle_at_bottom,_rgba(16,185,129,0.25),transparent_30%)]" /><div className="relative flex h-full flex-col justify-between"><Link to="/" className="text-3xl font-black">XPARF</Link><div><p className="mb-4 inline-flex rounded-full border border-white/10 bg-white/10 px-4 py-2 text-sm font-bold">ERP modernization</p><h2 className="text-5xl font-black leading-tight">{sideTitle ?? 'Kelola bisnis dari satu dashboard.'}</h2><p className="mt-5 max-w-lg text-lg leading-8 text-slate-300">{sideText ?? 'Multi branch, stock ledger, QRIS coin topup, sales/purchase posting, report, dan thermal receipt.'}</p></div><p className="text-sm text-slate-400">Great.ERP.Website → XPARF</p></div></section>
    <section className="flex items-center justify-center bg-slate-100 px-4 py-10"><div className="w-full max-w-md rounded-3xl bg-white p-8 shadow-xl"><Link to="/" className="mb-8 inline-block text-sm font-bold text-indigo-600">← Landing page</Link><h1 className="text-3xl font-black text-slate-900">{title}</h1><p className="mb-8 mt-2 text-slate-500">{subtitle}</p>{children}</div></section>
  </div>
}

type InputProps = InputHTMLAttributes<HTMLInputElement> & { label: string; error?: string }
export function Input({ label, error, ...props }: InputProps) {
  return <label className="block"><span className="text-sm font-semibold text-slate-700">{label}</span><input className="mt-1 w-full rounded-xl border border-slate-300 px-4 py-3 outline-none transition focus:border-indigo-500 focus:ring-4 focus:ring-indigo-100" {...props} />{error && <span className="mt-1 block text-sm text-red-600">{error}</span>}</label>
}
