export const PASSWORD_MIN_LENGTH = 8
export const PASSWORD_MAX_LENGTH = 128

/** Mirrors server-side {@link RegisterUserCommandValidator} password rules. */
export function getPasswordValidationErrors(password: string): string[] {
  const errors: string[] = []

  if (!password.length) {
    errors.push('Password is required.')
    return errors
  }

  if (password.length < PASSWORD_MIN_LENGTH) {
    errors.push(`Password must be at least ${PASSWORD_MIN_LENGTH} characters.`)
  }

  if (password.length > PASSWORD_MAX_LENGTH) {
    errors.push(`Password must be at most ${PASSWORD_MAX_LENGTH} characters.`)
  }

  if (!/[A-Z]/.test(password)) {
    errors.push('Password must contain at least one uppercase Latin letter.')
  }

  if (!/[a-z]/.test(password)) {
    errors.push('Password must contain at least one lowercase Latin letter.')
  }

  if (!/[0-9]/.test(password)) {
    errors.push('Password must contain at least one digit.')
  }

  if (!/[^a-zA-Z0-9]/.test(password)) {
    errors.push('Password must contain at least one special character.')
  }

  return errors
}

export function passwordsMatch(password: string, confirmPassword: string): boolean {
  return password === confirmPassword
}

export function getConfirmPasswordError(password: string, confirmPassword: string): string | null {
  if (!confirmPassword.length) {
    return 'Please confirm your password.'
  }

  if (!passwordsMatch(password, confirmPassword)) {
    return 'Passwords do not match.'
  }

  return null
}
