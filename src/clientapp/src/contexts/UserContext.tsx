import React, { createContext, useContext, useMemo, useCallback, ReactNode } from 'react'
import { useAuth0, User } from '@auth0/auth0-react'

/**
 * Shape of the user info object we’ll expose in context
 */
export interface UserInfo {
  id: string
  email?: string
  username?: string
  firstName?: string
  lastName?: string
  fullName?: string
  nickname?: string
  picture?: string
  emailVerified?: boolean
  permissions: string[]
  raw: User
}

/**
 * Context value shape
 */
export interface UserContextValue {
  isAuthenticated: boolean
  isLoading: boolean
  user: UserInfo | null

  getDisplayName: () => string
  getInitials: () => string
  getToken: () => Promise<string | null>

  hasPermission: (permission: string) => boolean
  hasAnyPermission: (permissions: string[]) => boolean
  hasAllPermissions: (permissions: string[]) => boolean
  hasRole: (role: string) => boolean

  username?: string
  email?: string
  fullName?: string
  firstName?: string
  lastName?: string
  permissions: string[]
  picture?: string
}

/**
 * Create the context
 */
const UserContext = createContext<UserContextValue | null>(null)

/**
 * Hook to consume the user context
 */
export const useUser = (): UserContextValue => {
  const context = useContext(UserContext)
  if (!context) {
    throw new Error('useUser must be used within a UserProvider')
  }
  return context
}

/**
 * Provider props
 */
interface UserProviderProps {
  children: ReactNode
}

/**
 * Provider component
 */
export const UserProvider: React.FC<UserProviderProps> = ({ children }) => {
  const { user, isAuthenticated, isLoading, getAccessTokenSilently } = useAuth0()

  const userInfo = useMemo<UserInfo | null>(() => {
    if (!isAuthenticated || !user) return null

    const firstName = user.given_name || user.name?.split(' ')[0] || ''
    const lastName = user.family_name || user.name?.split(' ').slice(1).join(' ') || ''
    const fullName = user.name || `${firstName} ${lastName}`.trim()

    const audience = 'https://vertical-slice.com/'
    const permissions = [
      ...((user.permissions as string[] | undefined) || []),
      ...((user[`${audience}permissions`] as string[] | undefined) || []),
      ...((user[`https://vertical-slice.com/permissions`] as string[] | undefined) || []),
      ...((user['https://verticalSlice/permissions'] as string[] | undefined) || []),
      ...((user.roles as string[] | undefined) || []),
      ...((user[`${audience}roles`] as string[] | undefined) || []),
      ...((user[`https://vertical-slice.com/roles`] as string[] | undefined) || []),
      ...((user['https://verticalSlice/roles'] as string[] | undefined) || []),
    ]

    const uniquePermissions = [...new Set(permissions)]

    return {
      id: user.sub!,
      email: user.email,
      username: user.email || user.nickname || user.preferred_username,
      firstName,
      lastName,
      fullName,
      nickname: user.nickname,
      picture: user.picture,
      emailVerified: user.email_verified,
      permissions: uniquePermissions,
      raw: user,
    }
  }, [user, isAuthenticated])

  const hasPermission = useCallback(
    (permission: string) => !!userInfo?.permissions?.includes(permission),
    [userInfo],
  )

  const hasAnyPermission = useCallback(
    (permissions: string[]) =>
      Array.isArray(permissions) && !!userInfo?.permissions?.some((p) => permissions.includes(p)),
    [userInfo],
  )

  const hasAllPermissions = useCallback(
    (permissions: string[]) =>
      Array.isArray(permissions) && !!userInfo?.permissions?.every((p) => permissions.includes(p)),
    [userInfo],
  )

  const getDisplayName = useCallback((): string => {
    if (!userInfo) return 'Guest'
    return userInfo.fullName || userInfo.nickname || userInfo.username || userInfo.email || 'User'
  }, [userInfo])

  const getInitials = useCallback((): string => {
    if (!userInfo) return 'G'
    if (userInfo.firstName && userInfo.lastName) {
      return `${userInfo.firstName[0]}${userInfo.lastName[0]}`.toUpperCase()
    }
    const parts = getDisplayName().split(' ')
    if (parts.length >= 2) {
      return `${parts[0][0]}${parts[1][0]}`.toUpperCase()
    }
    return getDisplayName().charAt(0).toUpperCase()
  }, [userInfo, getDisplayName])

  const getToken = useCallback(async (): Promise<string | null> => {
    try {
      return await getAccessTokenSilently()
    } catch (error) {
      console.error('Failed to get access token:', error)
      return null
    }
  }, [getAccessTokenSilently])

  const hasRole = useCallback((role: string) => hasPermission(role), [hasPermission])

  const contextValue: UserContextValue = {
    isAuthenticated,
    isLoading,
    user: userInfo,
    getDisplayName,
    getInitials,
    getToken,
    hasPermission,
    hasAnyPermission,
    hasAllPermissions,
    hasRole,
    username: userInfo?.username,
    email: userInfo?.email,
    fullName: userInfo?.fullName,
    firstName: userInfo?.firstName,
    lastName: userInfo?.lastName,
    permissions: userInfo?.permissions || [],
    picture: userInfo?.picture,
  }

  return <UserContext.Provider value={contextValue}>{children}</UserContext.Provider>
}

/**
 * Higher-order component for components that need user context
 */
export const withUser = <P extends object>(
  WrappedComponent: React.ComponentType<P>,
): React.FC<P> => {
  const WithUserComponent: React.FC<P> = (props) => (
    <UserProvider>
      <WrappedComponent {...props} />
    </UserProvider>
  )
  return WithUserComponent
}

/**
 * Permission-based render guard
 */
interface RequirePermissionProps {
  permission?: string
  permissions?: string[]
  requireAll?: boolean
  children: ReactNode
  fallback?: ReactNode
}

export const RequirePermission: React.FC<RequirePermissionProps> = ({
  permission,
  permissions,
  requireAll = false,
  children,
  fallback = null,
}) => {
  const { hasPermission, hasAnyPermission, hasAllPermissions } = useUser()

  const hasAccess = permission
    ? hasPermission(permission)
    : permissions
      ? requireAll
        ? hasAllPermissions(permissions)
        : hasAnyPermission(permissions)
      : false

  return <>{hasAccess ? children : fallback}</>
}

export default UserProvider
