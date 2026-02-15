// jest-dom adds custom jest matchers for asserting on DOM nodes.
// allows you to do things like:
// expect(element).toHaveTextContent(/react/i)
// learn more: https://github.com/testing-library/jest-dom
import '@testing-library/jest-dom'
import { vi } from 'vitest'

// ============================================================================
// Global Mocks
// ============================================================================

// Mock Auth0 at global level
vi.mock('@auth0/auth0-react', () => ({
  useAuth0: vi.fn(() => ({
    user: {
      name: 'Test User',
      given_name: 'Test',
      picture: 'https://example.com/avatar.jpg',
    },
    isAuthenticated: true,
    isLoading: false,
    error: null,
    getAccessTokenSilently: vi.fn().mockResolvedValue('mock-token'),
  })),
  Auth0Provider: ({ children }) => children,
}))

// Mock CoreUI icons-react
vi.mock('@coreui/icons-react', () => {
  return {
    __esModule: true,
    default: function CIcon(props) {
      // Simple mock that doesn't use React.createElement directly
      return null
    },
  }
})

// Mock CoreUI icons package
vi.mock('@coreui/icons', () => ({
  cilSave: 'cilSave',
  cilArrowLeft: 'cilArrowLeft',
  cilTrash: 'cilTrash',
  cilGraph: 'cilGraph',
  cilSitemap: 'cilSitemap',
  cilReload: 'cilReload',
  cilGlobeAlt: 'cilGlobeAlt',
  cilMap: 'cilMap',
  cilCheck: 'cilCheck',
  cilX: 'cilX',
  cilShieldAlt: 'cilShieldAlt',
  cilBell: 'cilBell',
  cilHistory: 'cilHistory',
}))

// ============================================================================
// Browser API Mocks
// ============================================================================

// Mock window.matchMedia
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: vi.fn().mockImplementation((query) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: vi.fn(), // deprecated
    removeListener: vi.fn(), // deprecated
    addEventListener: vi.fn(),
    removeEventListener: vi.fn(),
    dispatchEvent: vi.fn(),
  })),
})

// Mock window.confirm
Object.defineProperty(window, 'confirm', {
  writable: true,
  value: vi.fn().mockReturnValue(true),
})

// Mock localStorage
const localStorageMock = {
  getItem: vi.fn(),
  setItem: vi.fn(),
  removeItem: vi.fn(),
  clear: vi.fn(),
}
Object.defineProperty(window, 'localStorage', {
  value: localStorageMock,
})

// Mock ResizeObserver
class ResizeObserverMock {
  observe() {}
  unobserve() {}
  disconnect() {}
}
window.ResizeObserver = ResizeObserverMock

// Mock IntersectionObserver
class IntersectionObserverMock {
  constructor() {}
  observe() {}
  unobserve() {}
  disconnect() {}
}
window.IntersectionObserver = IntersectionObserverMock

// Suppress console errors during tests (optional - comment out for debugging)
// vi.spyOn(console, 'error').mockImplementation(() => {})
