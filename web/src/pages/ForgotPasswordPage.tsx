import type { FormEvent } from 'react'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { api, getApiErrorMessage } from '../lib/api'
import { AuthShell, Input } from './LoginPage'

export function ForgotPasswordPage() {
  const [email, setEmail] = useState('')
  const [newPassword, setNewPassword] = useState('')
  const [mode, setMode] = useState<'forgot' | 'reset'>('forgot')
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  const submit = async (event: FormEvent) => {
    event.preventDefault()
    setLoading(true)
    setError('')
    setMessage('')
    try {
      if (mode === 'forgot') {
        await api.post('/auth/forgot-password', { email: email.trim().toLowerCase() })
        setMessage('Jika email terdaftar, instruksi reset password diproses. Untuk testing lokal, gunakan tab Reset manual.')
      } else {
        await api.post('/auth/reset-password', { email: email.trim().toLowerCase(), newPassword })
        setMessage('Password berhasil direset. Silakan login.')
      }
    } catch (err) {
      setError(getApiErrorMessage(err, 'Request gagal. Cek email/password atau koneksi API.'))
    } finally {
      setLoading(false)
    }
  }

  return (
    <AuthShell title="Reset Password" subtitle="Pulihkan akses akun XPARF" sideTitle="Tidak perlu panik" sideText="Flow forgot/reset password sudah bisa diuji dari halaman ini memakai endpoint auth API.">
      <div className="mb-5 grid grid-cols-2 rounded-2xl bg-slate-100 p-1 text-sm font-bold">
        <button className={`rounded-xl py-2 ${mode === 'forgot' ? 'bg-white text-indigo-600 shadow-sm' : 'text-slate-500'}`} onClick={() => setMode('forgot')}>Forgot</button>
        <button className={`rounded-xl py-2 ${mode === 'reset' ? 'bg-white text-indigo-600 shadow-sm' : 'text-slate-500'}`} onClick={() => setMode('reset')}>Reset manual</button>
      </div>
      <form className="space-y-4" onSubmit={submit}>
        {message && <p className="rounded-xl bg-emerald-50 p-3 text-sm font-medium text-emerald-700">{message}</p>}
        {error && <p className="rounded-xl bg-red-50 p-3 text-sm font-medium text-red-700">{error}</p>}
        <Input label="Email" type="email" autoComplete="email" value={email} onChange={(event) => setEmail(event.target.value)} required />
        {mode === 'reset' && <Input label="Password baru" type="password" autoComplete="new-password" value={newPassword} onChange={(event) => setNewPassword(event.target.value)} required minLength={8} />}
        <button disabled={loading || !email || (mode === 'reset' && newPassword.length < 8)} className="w-full rounded-xl bg-indigo-600 px-4 py-3 font-semibold text-white hover:bg-indigo-700 disabled:opacity-60">{loading ? 'Memproses...' : mode === 'forgot' ? 'Kirim instruksi reset' : 'Reset password'}</button>
        <p className="text-center text-sm text-slate-500"><Link className="font-semibold text-indigo-600" to="/login">Kembali login</Link></p>
      </form>
    </AuthShell>
  )
}
