export type SessionUser = {
  roles: string[]
  tenantId?: string
  orgUnitId?: string
  userType?: string
}

export function canViewGlobal(user: SessionUser) {
  return user.roles.includes('PlatformAdmin')
}

export function canViewSourceQueue(user: SessionUser) {
  return user.roles.includes('OrgUnitAdmin')
}

export function canViewDestinationQueue(user: SessionUser) {
  return user.roles.includes('OrgUnitAdmin')
}

export function canUploadKyc(user: SessionUser) {
  return user.roles.includes('EndUser')
}

export function canViewMyReports(user: SessionUser) {
  return user.roles.includes('EndUser')
}