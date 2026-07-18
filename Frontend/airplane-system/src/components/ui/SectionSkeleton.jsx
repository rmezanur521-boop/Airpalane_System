// src/components/ui/SectionSkeleton.jsx
const GRID_COLS = { 2: 'lg:grid-cols-2', 3: 'lg:grid-cols-3', 4: 'lg:grid-cols-4' };

export default function SectionSkeleton({ cards = 4, imageHeight = 'h-40' }) {
  return (
    <div className="page-container py-14">
      <div className="h-7 w-56 bg-slate-200 rounded-lg mb-6 animate-pulse" />
      <div className={`grid grid-cols-1 sm:grid-cols-2 ${GRID_COLS[cards] || 'lg:grid-cols-4'} gap-6`}>
        {Array.from({ length: cards }).map((_, i) => (
          <div key={i} className="rounded-2xl overflow-hidden border border-slate-100">
            <div className={`${imageHeight} bg-slate-200 animate-pulse`} />
            <div className="p-4 space-y-2">
              <div className="h-4 bg-slate-200 rounded animate-pulse w-3/4" />
              <div className="h-3 bg-slate-100 rounded animate-pulse w-1/2" />
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}