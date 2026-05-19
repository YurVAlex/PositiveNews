import { describe, expect, it } from 'vitest'
import { resolveSourceDefaultImageTag } from './source-default-image'

describe('resolveSourceDefaultImageTag', () => {
  it('returns Defaults path for known sources', () => {
    expect(resolveSourceDefaultImageTag('NASA Breaking News')).toContain('/Defaults/nasa.png')
    expect(resolveSourceDefaultImageTag('tiny buddha')).toContain('/Defaults/buddha.png')
  })

  it('returns null for unknown sources', () => {
    expect(resolveSourceDefaultImageTag('Positive Source')).toBeNull()
  })
})
