import { describe, expect, it } from 'vitest'
import {
  getConfirmPasswordError,
  getPasswordValidationErrors,
  passwordsMatch,
} from './password-validation'

describe('password-validation', () => {
  it('accepts a password that meets complexity rules', () => {
    expect(getPasswordValidationErrors('Password1!')).toEqual([])
  })

  it('reports missing complexity requirements', () => {
    expect(getPasswordValidationErrors('short')).toEqual([
      'Password must be at least 8 characters.',
      'Password must contain at least one uppercase Latin letter.',
      'Password must contain at least one digit.',
      'Password must contain at least one special character.',
    ])
  })

  it('detects password mismatch', () => {
    expect(passwordsMatch('Password1!', 'Password1?')).toBe(false)
    expect(getConfirmPasswordError('Password1!', 'Password1?')).toBe('Passwords do not match.')
  })

  it('requires confirm password input', () => {
    expect(getConfirmPasswordError('Password1!', '')).toBe('Please confirm your password.')
  })
})
