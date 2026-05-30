import { useState } from 'react'
import { Link } from 'react-router-dom'
import { api } from '../lib/api'
import { AuthShell, Input } from './LoginPage'

export function ForgotPasswordPage() {
  const [email, setEmail] = useState('')
  const [message, setMessage] = useState('')
  const [loading, setLoading] = useState(false)
  const submit = async (event: React.FormEvent) => {
    event.preventDefault()
    setLoading(true)
    try {
      await api.post('/auth/forgot-password', { email })
      setMessage('Jika email terdaftar, link reset password akan dikirim.')
    } finally {
      setLoading(false)
    }
  }
  return (
    <AuthShell title="Forgot Password" subtitle="Reset password akun XPARF">
      <form className="space-y-4" onSubmit={submit}>
        <Input label="Email" type="email" value={email} onChange={(event) => setEmail(event.target.value)} />
        <button disabled={loading} className="w-full rounded-xl bg-indigo-600 px-4 py-3 font-semibold text-white hover:bg-indigo-700 disabled:opacity-60">Kirim reset link</button>
        {message && <p className="rounded-xl bg-emerald-50 p-3 text-sm text-emerald-700">{message}</p>}
        <p className="text-center text-sm text-slate-500"><Link className="font-semibold text-indigo-600" to="/login">Kembali login</Link></p>
      </form>
    </AuthShell>
  )
}
