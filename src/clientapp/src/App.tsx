import React, { Suspense, useEffect } from 'react'
import { HashRouter, Route, Routes } from 'react-router-dom'
import { useSelector } from 'react-redux'
import type { RootState } from './store'
import { CSpinner, useColorModes } from '@coreui/react'
import './scss/style.scss'
import { useAuth0 } from '@auth0/auth0-react'
import { UserProvider } from './contexts/UserContext'
import { TelemetryProvider } from './services/telemetry/TelemetryProvider'

// Containers
const DefaultLayout = React.lazy(() => import('./layout/DefaultLayout.tsx'))

const App = () => {
  const { error } = useAuth0()

  const { isColorModeSet, setColorMode } = useColorModes('vertical-slice')
  const storedTheme = useSelector((state: RootState) => state.theme)

  useEffect(() => {
    const urlParams = new URLSearchParams(window.location.href.split('?')[1])
    const themeParam = urlParams.get('theme')
    const theme = themeParam?.match(/^[A-Za-z0-9\s]+/)?.[0]
    if (theme) {
      setColorMode(theme)
    }

    if (isColorModeSet()) {
      return
    }

    setColorMode(storedTheme)
  }, [setColorMode, isColorModeSet, storedTheme]) // Add proper dependencies

  if (error) {
    return <div>Oops... {error.message}</div>
  }

  return (
    <TelemetryProvider>
      <UserProvider>
        <HashRouter>
          <Suspense
            fallback={
              <div className="pt-3 text-center">
                <CSpinner color="primary" variant="grow" />
              </div>
            }
          >
            <Routes>
              <Route path="*" element={<DefaultLayout />} />
            </Routes>
          </Suspense>
        </HashRouter>
      </UserProvider>
    </TelemetryProvider>
  )
}

export default App
