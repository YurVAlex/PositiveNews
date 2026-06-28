import type { ComponentType } from 'react'
import { ArticlesModeration } from './ArticlesModeration'
import { AuditLogs } from './AuditLogs'
import { IngestionRuns } from './IngestionRuns'
import { ManageComments } from './ManageComments'
import { ManageUsers } from './ManageUsers'
import { SourcesModeration } from './SourcesModeration'

export type AdminSectionId =
  | 'sources'
  | 'articles'
  | 'users'
  | 'comments'
  | 'audit'
  | 'ingestion'

export type AdminSectionConfig = {
  id: AdminSectionId
  label: string
  Component: ComponentType
}

export const ADMIN_SECTIONS: AdminSectionConfig[] = [
  { id: 'sources', label: 'Sources', Component: SourcesModeration },
  { id: 'articles', label: 'Articles', Component: ArticlesModeration },
  { id: 'users', label: 'Users', Component: ManageUsers },
  { id: 'comments', label: 'Comments', Component: ManageComments },
  { id: 'audit', label: 'Audit', Component: AuditLogs },
  { id: 'ingestion', label: 'Ingestion', Component: IngestionRuns },
]
