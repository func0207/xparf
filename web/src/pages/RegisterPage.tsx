import { useForm } from 'react-hook-form'
import { Link, useNavigate } from 'react-router-dom'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { api, type AuthResponse } from '../lib/api'
import { useAuthStore } from '../store/auth-store'
import { AuthShell, Input } from './LoginPage'

const schema = z.object({
  companyName: z.string().min(2),
  email: z.email(),
  userName: z.string().min(3),
  password: z.string().min(8),
  phone: z.string().optional(),
  address: z.string().optional(),
})
type FormValues = z.infer<typeof schema>

export function RegisterPage() {
  const navigate = useNavigate()
  const setAuth = useAuthStore((state) => state.setAuth)
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<FormValues>({ resolver: zodResolver(schema) })
  const onSubmit = async (values: FormValues) => {
    const { data } = await api.post<AuthResponse>('/auth/register-company', values)
    setAuth(data)
    navigate('/')
  }
  return (
    <AuthShell title="Register Company" subtitle="Buat tenant dan owner account">
      <form className="space-y-4" onSubmit={handleSubmit(onSubmit)}>
        <Input label="Nama company" error={errors.companyName?.message} {...register('companyName')} />
        <Input label="Email" type="email" error={errors.email?.message} {...register('email')} />
        <Input label="Username" error={errors.userName?.message} {...register('userName')} />
        <Input label="Password" type="password" error={errors.password?.message} {...register('password')} />
        <Input label="Phone" error={errors.phone?.message} {...register('phone')} />
        <Input label="Address" error={errors.address?.message} {...register('address')} />
        <button disabled={isSubmitting} className="w-full rounded-xl bg-indigo-600 px-4 py-3 font-semibold text-white hover:bg-indigo-700 disabled:opacity-60">Register</button>
        <p className="text-center text-sm text-slate-500">Sudah punya akun? <Link className="font-semibold text-indigo-600" to="/login">Login</Link></p>
      </form>
    </AuthShell>
  )
}
