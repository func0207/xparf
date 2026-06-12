import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useMemo, useState } from 'react'
import { ConfirmDialog } from '../components/ConfirmDialog'
import type { PageResponse } from '../components/DataTable'
import { StatCard } from '../components/StatCard'
import { api, getApiErrorMessage } from '../lib/api'

type Company = { id: number; name: string; email: string; phone?: string; address?: string; subscriptionPlan: number; coinBalance: number; isFrozen: boolean }
type User = { id: number; email: string; userName: string; roleId: number; roleName: string; emailConfirmed: boolean; isOwner: boolean; isActive: boolean; branchIds: number[] }
type Role = { id: number; name: string; description?: string; isSystemRole: boolean; permissions: string[] }
type Permission = { id: number; code: string; name: string; module: string }
type Branch = { id: number; code: string; name: string; address?: string; phone?: string; isActive: boolean }
type EmployeeForm = { id?: number; email: string; userName: string; password: string; roleId: number; branchIdsText: string; isActive: boolean }

const employeeDefaults: EmployeeForm = { email: '', userName: '', password: '', roleId: 0, branchIdsText: '', isActive: true }
const number = new Intl.NumberFormat('id-ID')

export function SettingsPage() {
  const queryClient = useQueryClient()
  const companyQuery = useQuery({ queryKey: ['company', 'me'], queryFn: async () => (await api.get<Company>('/company/me')).data })
  const usersQuery = useQuery({ queryKey: ['users'], queryFn: async () => (await api.get<User[]>('/users')).data })
  const rolesQuery = useQuery({ queryKey: ['roles'], queryFn: async () => (await api.get<Role[]>('/roles')).data })
  const permissionsQuery = useQuery({ queryKey: ['roles', 'permissions'], queryFn: async () => (await api.get<Permission[]>('/roles/permissions')).data })
  const branchesQuery = useQuery({ queryKey: ['branches', 'settings'], queryFn: async () => (await api.get<PageResponse<Branch>>('/branches', { params: { page: 1, pageSize: 100, sortBy: 'name', sortDirection: 'asc' } })).data.items })

  const [companyForm, setCompanyForm] = useState({ name: '', phone: '', address: '' })
  const [employeeForm, setEmployeeForm] = useState<EmployeeForm>(employeeDefaults)
  const [isEmployeeOpen, setIsEmployeeOpen] = useState(false)
  const [selectedRoleId, setSelectedRoleId] = useState<number | null>(null)
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')

  useEffect(() => {
    if (companyQuery.data) setCompanyForm({ name: companyQuery.data.name, phone: companyQuery.data.phone ?? '', address: companyQuery.data.address ?? '' })
  }, [companyQuery.data])

  const defaultRoleId = useMemo(() => rolesQuery.data?.[0]?.id ?? 0, [rolesQuery.data])
  const selectedRole = rolesQuery.data?.find((role) => role.id === (selectedRoleId ?? defaultRoleId))

  const updateCompany = useMutation({
    mutationFn: async () => (await api.put<Company>('/company/me', companyForm)).data,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['company'] })
      setMessage('Company profile tersimpan.')
    },
    onError: (err) => setError(getApiErrorMessage(err, 'Gagal update company.')),
  })

  const saveEmployee = useMutation({
    mutationFn: async () => {
      const branchIds = parseIds(employeeForm.branchIdsText)
      if (employeeForm.id) return api.put(`/users/${employeeForm.id}`, { userName: employeeForm.userName.trim(), isActive: employeeForm.isActive })
      return api.post('/users', {
        email: employeeForm.email.trim().toLowerCase(),
        userName: employeeForm.userName.trim(),
        password: employeeForm.password,
        roleId: employeeForm.roleId || defaultRoleId,
        branchIds,
        isActive: employeeForm.isActive,
      })
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['users'] })
      closeEmployeeForm()
      setMessage('Employee tersimpan.')
    },
    onError: (err) => setError(getApiErrorMessage(err, 'Gagal simpan employee.')),
  })

  const inviteEmployee = useMutation({
    mutationFn: async () => api.post('/users/invite', {
      email: employeeForm.email.trim().toLowerCase(),
      userName: employeeForm.userName.trim(),
      roleId: employeeForm.roleId || defaultRoleId,
      branchIds: parseIds(employeeForm.branchIdsText),
    }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['users'] })
      closeEmployeeForm()
      setMessage('Invite employee dibuat.')
    },
    onError: (err) => setError(getApiErrorMessage(err, 'Gagal invite employee.')),
  })

  const deleteEmployee = useMutation({
    mutationFn: async (userId: number) => api.delete(`/users/${userId}`),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['users'] })
      setMessage('Employee dihapus.')
    },
    onError: (err) => setError(getApiErrorMessage(err, 'Gagal hapus employee.')),
  })

  const updateRole = useMutation({
    mutationFn: async ({ userId, roleId }: { userId: number; roleId: number }) => api.put(`/users/${userId}/roles`, { roleId }),
    onSuccess: async () => queryClient.invalidateQueries({ queryKey: ['users'] }),
    onError: (err) => setError(getApiErrorMessage(err, 'Gagal update role user.')),
  })

  const updateBranches = useMutation({
    mutationFn: async ({ userId, branchIdsText }: { userId: number; branchIdsText: string }) => {
      const branchIds = parseIds(branchIdsText)
      return api.put(`/users/${userId}/branches`, { branchIds, defaultBranchId: branchIds[0] ?? null })
    },
    onSuccess: async () => queryClient.invalidateQueries({ queryKey: ['users'] }),
    onError: (err) => setError(getApiErrorMessage(err, 'Gagal update cabang user.')),
  })

  const updateRolePermissions = useMutation({
    mutationFn: async ({ roleId, permissionIds }: { roleId: number; permissionIds: number[] }) => api.put(`/roles/${roleId}/permissions`, { permissionIds }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['roles'] })
      setMessage('Permission role tersimpan.')
    },
    onError: (err) => setError(getApiErrorMessage(err, 'Gagal update permission role.')),
  })

  function openCreateEmployee() {
    setEmployeeForm({ ...employeeDefaults, roleId: defaultRoleId })
    setIsEmployeeOpen(true)
  }

  function openEditEmployee(user: User) {
    setEmployeeForm({ id: user.id, email: user.email, userName: user.userName, password: '', roleId: user.roleId, branchIdsText: user.branchIds.join(','), isActive: user.isActive })
    setIsEmployeeOpen(true)
  }

  function closeEmployeeForm() {
    setEmployeeForm(employeeDefaults)
    setIsEmployeeOpen(false)
  }

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-bold">Settings</h2>
        <p className="text-slate-500">Company profile, subscription, employee, role, dan permission matrix.</p>
      </div>

      {(message || error) && (
        <div className={`rounded-2xl p-4 text-sm font-medium ${error ? 'bg-red-50 text-red-700' : 'bg-emerald-50 text-emerald-700'}`}>
          {error || message}
          <button className="float-right font-bold" onClick={() => { setError(''); setMessage('') }}>×</button>
        </div>
      )}

      <div className="grid gap-4 md:grid-cols-3">
        <StatCard title="Company" value={companyQuery.data?.name ?? '-'} description={companyQuery.data?.email ?? ''} />
        <StatCard title="Coin balance" value={number.format(companyQuery.data?.coinBalance ?? 0)} description={companyQuery.data?.isFrozen ? 'Frozen' : 'Aktif'} />
        <StatCard title="Employees" value={number.format(usersQuery.data?.length ?? 0)} description="User aktif/nonaktif" />
      </div>

      <section className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
        <h3 className="text-xl font-bold">Company profile</h3>
        <div className="mt-5 grid gap-4 md:grid-cols-3">
          <Input label="Name" value={companyForm.name} onChange={(value) => setCompanyForm({ ...companyForm, name: value })} />
          <Input label="Phone" value={companyForm.phone} onChange={(value) => setCompanyForm({ ...companyForm, phone: value })} />
          <Input label="Address" value={companyForm.address} onChange={(value) => setCompanyForm({ ...companyForm, address: value })} />
        </div>
        <button disabled={updateCompany.isPending} className="mt-5 rounded-xl bg-indigo-600 px-4 py-2 font-semibold text-white hover:bg-indigo-700 disabled:opacity-60" onClick={() => updateCompany.mutate()}>{updateCompany.isPending ? 'Saving...' : 'Save company'}</button>
      </section>

      <section className="rounded-2xl border border-slate-200 bg-white shadow-sm">
        <div className="flex items-center justify-between border-b border-slate-200 p-5">
          <div>
            <h3 className="text-xl font-bold">Employees</h3>
            <p className="text-sm text-slate-500">CRUD user, invite, assign role, assign branch.</p>
          </div>
          <button className="rounded-xl bg-indigo-600 px-4 py-2 font-semibold text-white hover:bg-indigo-700" onClick={openCreateEmployee}>Tambah user</button>
        </div>
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-slate-200 text-sm">
            <thead className="bg-slate-50"><tr><th className="px-5 py-3 text-left">User</th><th className="px-5 py-3 text-left">Role</th><th className="px-5 py-3 text-left">Branches</th><th className="px-5 py-3 text-left">Status</th><th className="px-5 py-3 text-left">Aksi</th></tr></thead>
            <tbody className="divide-y divide-slate-100">
              {usersQuery.isLoading && <tr><td className="px-5 py-8 text-center text-slate-500" colSpan={5}>Loading employees...</td></tr>}
              {usersQuery.data?.map((user) => (
                <tr key={user.id} className="hover:bg-slate-50">
                  <td className="px-5 py-3"><p className="font-semibold">{user.userName}</p><p className="text-slate-500">{user.email}</p>{user.isOwner && <span className="mt-1 inline-block rounded-full bg-amber-50 px-2 py-1 text-xs font-semibold text-amber-700">Owner</span>}</td>
                  <td className="px-5 py-3"><select className="rounded-lg border border-slate-300 px-3 py-2" value={user.roleId} onChange={(event) => updateRole.mutate({ userId: user.id, roleId: Number(event.target.value) })}>{rolesQuery.data?.map((role) => <option key={role.id} value={role.id}>{role.name}</option>)}</select></td>
                  <td className="px-5 py-3"><input className="w-48 rounded-lg border border-slate-300 px-3 py-2" defaultValue={user.branchIds.join(',')} onBlur={(event) => updateBranches.mutate({ userId: user.id, branchIdsText: event.target.value })} title={branchesQuery.data?.map((branch) => `${branch.id}: ${branch.name}`).join('\n')} /></td>
                  <td className="px-5 py-3">{user.isActive ? <span className="rounded-full bg-emerald-50 px-2 py-1 text-xs font-semibold text-emerald-700">Aktif</span> : <span className="rounded-full bg-slate-100 px-2 py-1 text-xs font-semibold text-slate-600">Nonaktif</span>}</td>
                  <td className="px-5 py-3"><div className="flex gap-2"><button className="rounded-lg border px-3 py-1 text-xs font-semibold hover:bg-slate-100" onClick={() => openEditEmployee(user)}>Edit</button><ConfirmDialog title="Hapus user?" description={`User ${user.userName} akan dihapus.`} onConfirm={() => deleteEmployee.mutate(user.id)}><button disabled={user.isOwner} className="rounded-lg border border-red-200 px-3 py-1 text-xs font-semibold text-red-600 hover:bg-red-50 disabled:opacity-40">Hapus</button></ConfirmDialog></div></td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>

      <section className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
        <div className="flex flex-wrap items-center justify-between gap-4">
          <div>
            <h3 className="text-xl font-bold">Role permission matrix</h3>
            <p className="text-sm text-slate-500">Atur akses role per module dan permission.</p>
          </div>
          <select className="rounded-xl border border-slate-300 px-4 py-2" value={selectedRole?.id ?? ''} onChange={(event) => setSelectedRoleId(Number(event.target.value))}>
            {rolesQuery.data?.map((role) => <option key={role.id} value={role.id}>{role.name}{role.isSystemRole ? ' (system)' : ''}</option>)}
          </select>
        </div>
        <RolePermissionMatrix role={selectedRole} permissions={permissionsQuery.data ?? []} disabled={updateRolePermissions.isPending || !selectedRole} onSave={(permissionIds) => selectedRole && updateRolePermissions.mutate({ roleId: selectedRole.id, permissionIds })} />
      </section>

      {isEmployeeOpen && <EmployeePanel form={employeeForm} roles={rolesQuery.data ?? []} branches={branchesQuery.data ?? []} isSaving={saveEmployee.isPending || inviteEmployee.isPending} onChange={setEmployeeForm} onClose={closeEmployeeForm} onSave={() => saveEmployee.mutate()} onInvite={() => inviteEmployee.mutate()} />}
    </div>
  )
}

function parseIds(value: string) {
  return value.split(',').map((entry) => Number(entry.trim())).filter(Boolean)
}

function Input({ label, value, onChange }: { label: string; value: string; onChange: (value: string) => void }) {
  return <label className="block"><span className="text-sm font-medium text-slate-700">{label}</span><input className="mt-1 w-full rounded-xl border border-slate-300 px-4 py-3 outline-none focus:border-indigo-500" value={value} onChange={(event) => onChange(event.target.value)} /></label>
}

function RolePermissionMatrix({ role, permissions, disabled, onSave }: { role?: Role; permissions: Permission[]; disabled: boolean; onSave: (permissionIds: number[]) => void }) {
  const [selectedCodes, setSelectedCodes] = useState<string[]>([])

  useEffect(() => {
    setSelectedCodes(role?.permissions ?? [])
  }, [role?.id, role?.permissions])

  const grouped = useMemo(() => permissions.reduce<Record<string, Permission[]>>((acc, permission) => {
    acc[permission.module] = [...(acc[permission.module] ?? []), permission]
    return acc
  }, {}), [permissions])

  const permissionIds = permissions.filter((permission) => selectedCodes.includes(permission.code)).map((permission) => permission.id)

  return <div className="mt-5 space-y-5">
    {!role && <p className="rounded-xl bg-slate-50 p-4 text-sm text-slate-500">Pilih role terlebih dahulu.</p>}
    {role && Object.entries(grouped).map(([module, modulePermissions]) => (
      <div key={module} className="rounded-2xl border border-slate-200 p-4">
        <div className="mb-3 flex items-center justify-between">
          <h4 className="font-bold text-slate-800">{module}</h4>
          <span className="text-xs font-semibold text-slate-500">{modulePermissions.filter((permission) => selectedCodes.includes(permission.code)).length}/{modulePermissions.length}</span>
        </div>
        <div className="grid gap-2 md:grid-cols-2 xl:grid-cols-3">
          {modulePermissions.map((permission) => (
            <label key={permission.id} className="flex items-start gap-3 rounded-xl border border-slate-100 p-3 hover:bg-slate-50">
              <input type="checkbox" className="mt-1" checked={selectedCodes.includes(permission.code)} onChange={(event) => setSelectedCodes((current) => event.target.checked ? [...current, permission.code] : current.filter((code) => code !== permission.code))} />
              <span><span className="block text-sm font-semibold text-slate-800">{permission.name}</span><span className="block text-xs text-slate-500">{permission.code}</span></span>
            </label>
          ))}
        </div>
      </div>
    ))}
    <button disabled={disabled || !role} className="rounded-xl bg-indigo-600 px-4 py-2 font-semibold text-white hover:bg-indigo-700 disabled:opacity-60" onClick={() => onSave(permissionIds)}>Save permissions</button>
  </div>
}

function EmployeePanel({ form, roles, branches, isSaving, onChange, onClose, onSave, onInvite }: { form: EmployeeForm; roles: Role[]; branches: Branch[]; isSaving: boolean; onChange: (form: EmployeeForm) => void; onClose: () => void; onSave: () => void; onInvite: () => void }) {
  return (
    <div className="fixed inset-0 z-50 flex justify-end bg-slate-900/30">
      <div className="h-full w-full max-w-xl overflow-y-auto bg-white p-6 shadow-2xl">
        <div className="flex items-center justify-between border-b border-slate-200 pb-4"><h3 className="text-xl font-bold">{form.id ? 'Edit employee' : 'Tambah employee'}</h3><button className="rounded-lg border px-3 py-2 text-sm" onClick={onClose}>Tutup</button></div>
        <div className="mt-5 space-y-4">
          <Input label="Email" value={form.email} onChange={(email) => onChange({ ...form, email })} />
          <Input label="Username" value={form.userName} onChange={(userName) => onChange({ ...form, userName })} />
          {!form.id && <Input label="Password" value={form.password} onChange={(password) => onChange({ ...form, password })} />}
          <label className="block"><span className="text-sm font-medium text-slate-700">Role</span><select className="mt-1 w-full rounded-xl border border-slate-300 px-4 py-3" value={form.roleId} onChange={(event) => onChange({ ...form, roleId: Number(event.target.value) })}>{roles.map((role) => <option key={role.id} value={role.id}>{role.name}</option>)}</select></label>
          <label className="block"><span className="text-sm font-medium text-slate-700">Branch IDs</span><input className="mt-1 w-full rounded-xl border border-slate-300 px-4 py-3" value={form.branchIdsText} onChange={(event) => onChange({ ...form, branchIdsText: event.target.value })} placeholder="1,2" /><span className="mt-1 block text-xs text-slate-500">Available: {branches.map((branch) => `${branch.id}:${branch.name}`).join(', ') || '-'}</span></label>
          <label className="flex items-center gap-3"><input type="checkbox" checked={form.isActive} onChange={(event) => onChange({ ...form, isActive: event.target.checked })} /> Aktif</label>
          <div className="flex gap-3"><button disabled={isSaving} className="flex-1 rounded-xl bg-indigo-600 px-4 py-3 font-semibold text-white hover:bg-indigo-700 disabled:opacity-60" onClick={onSave}>{isSaving ? 'Saving...' : 'Save'}</button>{!form.id && <button disabled={isSaving} className="rounded-xl border border-indigo-200 px-4 py-3 font-semibold text-indigo-700 hover:bg-indigo-50 disabled:opacity-60" onClick={onInvite}>Invite</button>}</div>
        </div>
      </div>
    </div>
  )
}
