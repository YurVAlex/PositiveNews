import { ADMIN_SECTIONS, type AdminSectionId } from './admin-sections'

type AdminNavBarProps = {
  activeSection: AdminSectionId
  onSelect: (id: AdminSectionId) => void
}

export function AdminNavBar({ activeSection, onSelect }: AdminNavBarProps) {
  return (
    <nav className="d-flex flex-wrap gap-2 mb-4" aria-label="Admin sections">
      {ADMIN_SECTIONS.map((section) => (
        <button
          key={section.id}
          type="button"
          className={`btn btn-sm ${activeSection === section.id ? 'btn-primary' : 'btn-outline-secondary'}`}
          aria-pressed={activeSection === section.id}
          onClick={() => onSelect(section.id)}
        >
          {section.label}
        </button>
      ))}
    </nav>
  )
}
