import React, { createContext, useContext, useEffect, ReactNode } from 'react'
import { initializeTelemetry, TelemetryConfig } from './index'

interface TelemetryContextType {
  isEnabled: boolean
  config: TelemetryConfig
}

const TelemetryContext = createContext<TelemetryContextType | undefined>(undefined)

interface TelemetryProviderProps {
  children: ReactNode
  config?: TelemetryConfig
}

/**
 * Telemetry Provider component that initializes OpenTelemetry
 * and provides configuration context to child components
 */
export const TelemetryProvider: React.FC<TelemetryProviderProps> = ({ children, config = {} }) => {
  const isEnabled = config.enabled !== false && import.meta.env.VITE_TELEMETRY_ENABLED !== 'false'

  useEffect(() => {
    if (isEnabled) {
      initializeTelemetry(config)
    }
  }, [isEnabled]) // Remove config from dependencies to prevent re-initialization

  const contextValue: TelemetryContextType = {
    isEnabled,
    config,
  }

  return <TelemetryContext.Provider value={contextValue}>{children}</TelemetryContext.Provider>
}

/**
 * Hook to access telemetry context
 */
export const useTelemetry = (): TelemetryContextType => {
  const context = useContext(TelemetryContext)
  if (context === undefined) {
    throw new Error('useTelemetry must be used within a TelemetryProvider')
  }
  return context
}
