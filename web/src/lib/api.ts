import axios from 'axios'
import { useAuthStore } from '../store/auth-store'

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000/api',
})

api.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

export type AuthResponse = {
  accessToken: string
  refreshToken: string
  expiresAt: string
  userId: number
  companyId: number
  email: string
  userName: string
  isOwner: boolean
  isSuperAdmin: boolean
}

export type DashboardSummary = {
  salesTotal: number
  purchaseTotal: number
  salesCount: number
  purchaseCount: number
  lowStockCount: number
  coinBalance: number
}

export type CompanyProfile = {
  id: number
  name: string
  email: string
  phone?: string
  address?: string
  coinBalance: number
  isFrozen: boolean
}

export type TopupPackage = {
  id: number
  name: string
  moneyAmount: number
  coinAmount: number
  sortOrder: number
}
