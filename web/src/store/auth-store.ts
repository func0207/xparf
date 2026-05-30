import { create } from 'zustand'
import type { AuthResponse } from '../lib/api'

type AuthState = {
  accessToken: string | null
  refreshToken: string | null
  user: Pick<AuthResponse, 'userId' | 'companyId' | 'email' | 'userName' | 'isOwner' | 'isSuperAdmin'> | null
  setAuth: (response: AuthResponse) => void
  logout: () => void
}

const stored = localStorage.getItem('xparf.auth')
const initial = stored ? JSON.parse(stored) as Pick<AuthState, 'accessToken' | 'refreshToken' | 'user'> : null

export const useAuthStore = create<AuthState>((set) => ({
  accessToken: initial?.accessToken ?? null,
  refreshToken: initial?.refreshToken ?? null,
  user: initial?.user ?? null,
  setAuth: (response) => {
    const next = {
      accessToken: response.accessToken,
      refreshToken: response.refreshToken,
      user: {
        userId: response.userId,
        companyId: response.companyId,
        email: response.email,
        userName: response.userName,
        isOwner: response.isOwner,
        isSuperAdmin: response.isSuperAdmin,
      },
    }
    localStorage.setItem('xparf.auth', JSON.stringify(next))
    set(next)
  },
  logout: () => {
    localStorage.removeItem('xparf.auth')
    set({ accessToken: null, refreshToken: null, user: null })
  },
}))
