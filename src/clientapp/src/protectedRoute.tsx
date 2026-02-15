import React from 'react'
import { useAuth0 } from '@auth0/auth0-react'
import { useLocation } from 'react-router-dom'

const ProtectedRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isAuthenticated, isLoading, loginWithRedirect } = useAuth0()
  const location = useLocation()

  if (isLoading) {
    return <div>Loading...</div> // Or a loading spinner
  }

  if (!isAuthenticated) {
    loginWithRedirect({
      appState: {
        returnTo: location.pathname + location.search, // Preserve query params
      },
    })
    return null // Prevent further rendering
  }

  return children
}

export default ProtectedRoute
