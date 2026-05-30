import { useForm } from 'react-hook-form'
import { Link, useNavigate } from 'react-router-dom'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { api, type AuthResponse } from '../lib/api'
import { useAuthStore } from '../store/auth-store'

const schema = z.object({ email: z.email(), password: z.string().min(8) })
type FormValues = z.infer<typeof schema>

export function LoginPage() {
  const navigate = useNavigate()
  const setAuth = useAuthStore((state) => state.setAuth)
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<FormValues>({ resolver: zodResolver(schema) })

  const onSubmit = async (values: FormValues) => {
    const { data } = await api.post<AuthResponse>('/auth/login', values)
    setAuth(data)
    navigate('/')
  }

  return (
    <AuthShell title="Login XPARF" subtitle="Masuk ke dashboard ERP">
      <form className="space-y-4" onSubmit={handleSubmit(onSubmit)}>
        <Input label="Email" type="email" error={errors.email?.message} {...register('email')} />
        <Input label="Password" type="password" error={errors.password?.message} {...register('password')} />
        <button disabled={isSubmitting} className="w-full rounded-xl bg-indigo-600 px-4 py-3 font-semibold text-white hover:bg-indigo-700 disabled:opacity-60">Login</button>
        <p className="text-center text-sm text-slate-500">Belum punya akun? <Link className="font-semibold text-indigo-600" to="/register">Register company</Link></p>
      </form>
    </AuthShell>
  )
}

export function AuthShell({ title, subtitle, children }: { title: string; subtitle: string; children: React.ReactNode }) {
  return <div className="flex min-h-screen items-center justify-center bg-slate-100 px-4"><div className="w-full max-w-md rounded-3xl bg-white p-8 shadow-xl"><h1 className="text-3xl font-bold text-slate-900">{title}</h1><p className="mb-8 mt-2 text-slate-500">{subtitle}</p>{children}</div></div>
}

type InputProps = React.InputHTMLAttributes<HTMLInputElement> & { label: string; error?: string }
export function Input({ label, error, ...props }: InputProps) {
  return <label className="block"><span className="text-sm font-medium text-slate-700">{label}</span><input className="mt-1 w-full rounded-xl border border-slate-300 px-4 py-3 outline-none focus:border-indigo-500" {...props} />{error && <span className="mt-1 block text-sm text-red-600">{error}</span>}</label>
}
