/**
 * Centralized risk level resolver.
 *
 * Single source of truth for converting between the backend RiskLevel enum
 * (1-5), risk level labels, hex colors and CoreUI color names.
 *
 * Backend enum (RiskLevel.cs):
 *   VeryLow = 1, Low = 2, Medium = 3, High = 4, Critical = 5
 *
 * Risk Score (0-100) thresholds mirror the backend engine:
 *   >=90 Critical, >=75 High, >=50 Medium, >=25 Low, <25 VeryLow
 */

export type RiskLevelLabel = 'Critical' | 'High' | 'Medium' | 'Low' | 'VeryLow'

export type CuiColor = 'danger' | 'warning' | 'info' | 'success' | 'secondary'

export interface RiskLevelInfo {
  label: RiskLevelLabel
  displayLabel: string
  enumValue: number
  hexColor: string
  cuiColor: CuiColor
}

const LEVELS: readonly RiskLevelInfo[] = [
  { label: 'Critical', displayLabel: 'Critical', enumValue: 5, hexColor: '#dc3545', cuiColor: 'danger' },
  { label: 'High', displayLabel: 'High', enumValue: 4, hexColor: '#fd7e14', cuiColor: 'warning' },
  { label: 'Medium', displayLabel: 'Medium', enumValue: 3, hexColor: '#ffc107', cuiColor: 'info' },
  { label: 'Low', displayLabel: 'Low', enumValue: 2, hexColor: '#198754', cuiColor: 'success' },
  { label: 'VeryLow', displayLabel: 'Very Low', enumValue: 1, hexColor: '#6c757d', cuiColor: 'secondary' },
] as const

const BY_ENUM = new Map<number, RiskLevelInfo>(LEVELS.map((l) => [l.enumValue, l]))
const BY_LABEL = new Map<string, RiskLevelInfo>(LEVELS.map((l) => [l.label, l]))

const DEFAULT_LEVEL: RiskLevelInfo = BY_LABEL.get('VeryLow')!

/**
 * All risk level labels in severity order (highest first).
 */
export const RISK_LEVEL_LABELS: readonly RiskLevelLabel[] = LEVELS.map((l) => l.label)

// ---------------------------------------------------------------------------
// Resolvers
// ---------------------------------------------------------------------------

/**
 * Resolve a risk level from the backend enum value (1-5).
 */
export function fromEnum(value: number | undefined | null): RiskLevelInfo {
  if (value == null) return DEFAULT_LEVEL
  return BY_ENUM.get(value) ?? DEFAULT_LEVEL
}

/**
 * Resolve a risk level from a label string (e.g. "Critical", "VeryLow").
 */
export function fromLabel(label: string | undefined | null): RiskLevelInfo {
  if (!label) return DEFAULT_LEVEL
  return BY_LABEL.get(label) ?? DEFAULT_LEVEL
}

/**
 * Resolve a risk level from the composite Risk Score (0-100).
 * Thresholds match the backend RiskCalculationEngine:
 *   TotalRisk 0-1000 → >=900 Critical, >=750 High, >=500 Medium, >=250 Low
 *   RiskScore 0-100  → >=90 Critical,  >=75 High,  >=50 Medium,  >=25 Low
 */
export function fromRiskScore(score: number | undefined | null): RiskLevelInfo {
  if (score == null || score < 0) return DEFAULT_LEVEL
  if (score >= 90) return BY_LABEL.get('Critical')!
  if (score >= 75) return BY_LABEL.get('High')!
  if (score >= 50) return BY_LABEL.get('Medium')!
  if (score >= 25) return BY_LABEL.get('Low')!
  return DEFAULT_LEVEL
}

/**
 * Resolve from either an enum number, a label string, or undefined.
 * Convenience function for code that receives `riskLevel` from the API
 * which may be typed as `number | string | undefined`.
 */
export function resolve(value: number | string | undefined | null): RiskLevelInfo {
  if (value == null) return DEFAULT_LEVEL
  if (typeof value === 'number') return fromEnum(value)
  return fromLabel(value)
}

// ---------------------------------------------------------------------------
// Quick accessors
// ---------------------------------------------------------------------------

export function hexColor(value: number | string | undefined | null): string {
  return resolve(value).hexColor
}

export function cuiColor(value: number | string | undefined | null): CuiColor {
  return resolve(value).cuiColor
}

export function label(value: number | string | undefined | null): string {
  return resolve(value).label
}

export function displayLabel(value: number | string | undefined | null): string {
  return resolve(value).displayLabel
}
