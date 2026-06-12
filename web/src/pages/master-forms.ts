import type { FormField } from './CrudListPage'

export type ItemForm = {
  id?: number
  sku: string
  barcode: string
  name: string
  description: string
  itemType: number
  baseUnit: string
  isActive: boolean
  isDiscontinued: boolean
}

export type CustomerForm = {
  id?: number
  branchId: number | null
  code: string
  name: string
  phone: string
  email: string
  address: string
  creditLimit: number
  isActive: boolean
}

export type SupplierForm = {
  id?: number
  code: string
  name: string
  phone: string
  email: string
  address: string
  isActive: boolean
}

export type BranchForm = {
  id?: number
  code: string
  name: string
  phone: string
  address: string
  isActive: boolean
}

export const itemDefaults: ItemForm = { sku: '', barcode: '', name: '', description: '', itemType: 0, baseUnit: 'PCS', isActive: true, isDiscontinued: false }
export const customerDefaults: CustomerForm = { branchId: null, code: '', name: '', phone: '', email: '', address: '', creditLimit: 0, isActive: true }
export const supplierDefaults: SupplierForm = { code: '', name: '', phone: '', email: '', address: '', isActive: true }
export const branchDefaults: BranchForm = { code: '', name: '', phone: '', address: '', isActive: true }

export const itemFields: FormField<ItemForm>[] = [
  { key: 'sku', label: 'SKU', required: true },
  { key: 'barcode', label: 'Barcode' },
  { key: 'name', label: 'Nama barang', required: true },
  { key: 'description', label: 'Deskripsi' },
  { key: 'itemType', label: 'Tipe item', kind: 'select', options: [{ label: 'Stock item', value: 0 }, { label: 'Service', value: 1 }] },
  { key: 'baseUnit', label: 'Unit dasar', required: true },
  { key: 'isActive', label: 'Aktif', kind: 'checkbox' },
  { key: 'isDiscontinued', label: 'Discontinued', kind: 'checkbox' },
]

export const customerFields: FormField<CustomerForm>[] = [
  { key: 'code', label: 'Kode', required: true },
  { key: 'name', label: 'Nama konsumen', required: true },
  { key: 'phone', label: 'Phone' },
  { key: 'email', label: 'Email' },
  { key: 'address', label: 'Alamat' },
  { key: 'creditLimit', label: 'Limit kredit', kind: 'number' },
  { key: 'isActive', label: 'Aktif', kind: 'checkbox' },
]

export const supplierFields: FormField<SupplierForm>[] = [
  { key: 'code', label: 'Kode', required: true },
  { key: 'name', label: 'Nama distributor', required: true },
  { key: 'phone', label: 'Phone' },
  { key: 'email', label: 'Email' },
  { key: 'address', label: 'Alamat' },
  { key: 'isActive', label: 'Aktif', kind: 'checkbox' },
]

export const branchFields: FormField<BranchForm>[] = [
  { key: 'code', label: 'Kode', required: true },
  { key: 'name', label: 'Nama cabang', required: true },
  { key: 'phone', label: 'Phone' },
  { key: 'address', label: 'Alamat' },
  { key: 'isActive', label: 'Aktif', kind: 'checkbox' },
]

export function removeItemCreateOnly(value: ItemForm) {
  const { id: _id, ...rest } = value
  return rest
}

export function removeCodeAndId<T extends { id?: number; code?: string }>(value: T) {
  const { id: _id, code: _code, ...rest } = value
  return rest
}

export function removeId<T extends { id?: number }>(value: T) {
  const { id: _id, ...rest } = value
  return rest
}
