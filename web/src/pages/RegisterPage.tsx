import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Link, useNavigate } from 'react-router-dom'
import { api, getApiErrorMessage, type AuthResponse } from '../lib/api'
import { useAuthStore } from '../store/auth-store'
import { AuthShell, Input } from './LoginPage'

const schema = z.object({
  companyName: z.string().min(2, 'Nama company minimal 2 karakter'),
  email: z.string().email('Email tidak valid'),
  userName: z.string().min(3, 'Username minimal 3 karakter'),
  password: z.string().min(8, 'Password minimal 8 karakter'),
  phone: z.string().optional(),
  address: z.string().optional(),
})
type FormValues = z.infer<typeof schema>

export function RegisterPage() {
  const navigate = useNavigate()
  const setAuth = useAuthStore((state) => state.setAuth)
  const [error, setError] = useState('')
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<FormValues>({ resolver: zodResolver(schema) })

  const onSubmit = async (values: FormValues) => {
    setError('')
    try {
      const payload = {
        companyName: values.companyName.trim(),
        email: values.email.trim().toLowerCase(),
        userName: values.userName.trim(),
        password: values.password,
        phone: values.phone?.trim() || null,
        address: values.address?.trim() || null,
      }
      const { data } = await api.post<AuthResponse>('/auth/register-company', payload)
      setAuth(data)
      navigate('/dashboard', { replace: true })
    } catch (err) {
      setError(getApiErrorMessage(err, 'Register gagal. Email/company mungkin sudah dipakai atau API belum siap.'))
    }
  }

  return (
    <AuthShell title="Register Company" subtitle="Buat tenant dan owner account" sideTitle="Mulai test ERP baru" sideText="Company baru otomatis punya owner, main branch, role owner, dan akses awal untuk testing flow dashboard.">
      <form className="space-y-4" onSubmit={handleSubmit(onSubmit)}>
        {error && <p className="rounded-xl bg-red-50 p-3 text-sm font-medium text-red-700">{error}</p>}
        <Input label="Nama company" placeholder="PT Demo Retail" autoComplete="organization" error={errors.companyName?.message} {...register('companyName')} />
        <Input label="Email owner" type="email" placeholder="owner@company.com" autoComplete="email" error={errors.email?.message} {...register('email')} />
        <Input label="Username" placeholder="owner" autoComplete="username" error={errors.userName?.message} {...register('userName')} />
        <Input label="Password" type="password" placeholder="Minimal 8 karakter" autoComplete="new-password" error={errors.password?.message} {...register('password')} />
        <div className="grid gap-4 sm:grid-cols-2">
          <Input label="Phone" placeholder="08..." autoComplete="tel" error={errors.phone?.message} {...register('phone')} />
          <Input label="Address" placeholder="Alamat toko" autoComplete="street-address" error={errors.address?.message} {...register('address')} />
        </div>
        <button disabled={isSubmitting} className="w-full rounded-xl bg-indigo-600 px-4 py-3 font-semibold text-white hover:bg-indigo-700 disabled:opacity-60">{isSubmitting ? 'Mendaftarkan...' : 'Register & masuk'}</button>
        <p className="text-center text-sm text-slate-500">Sudah punya akun? <Link className="font-semibold text-indigo-600" to="/login">Login</Link></p>
      </form>
    </AuthShell>
  )
}
