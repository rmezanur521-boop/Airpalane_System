import { ChevronLeft, ChevronRight } from 'lucide-react';

export default function Pagination({ pageNumber, totalPages, onPage }) {
  if (totalPages <= 1) return null;

  const pages = Array.from({ length: totalPages }, (_, i) => i + 1);
  const visible = pages.filter(
    (p) => p === 1 || p === totalPages || Math.abs(p - pageNumber) <= 1
  );

  return (
    <div className="flex items-center justify-center gap-1 mt-4">
      <button
        onClick={() => onPage(pageNumber - 1)}
        disabled={pageNumber === 1}
        className="p-2 rounded-lg text-slate-500 hover:bg-slate-100 disabled:opacity-40 disabled:cursor-not-allowed transition"
      >
        <ChevronLeft className="h-4 w-4" />
      </button>

      {visible.reduce((acc, page, idx) => {
        if (idx > 0 && visible[idx - 1] !== page - 1) {
          acc.push(
            <span key={`ellipsis-${page}`} className="px-2 text-slate-400">
              …
            </span>
          );
        }
        acc.push(
          <button
            key={page}
            onClick={() => onPage(page)}
            className={`h-8 w-8 rounded-lg text-sm font-medium transition
              ${pageNumber === page
                ? 'bg-brand-600 text-white'
                : 'text-slate-600 hover:bg-slate-100'
              }`}
          >
            {page}
          </button>
        );
        return acc;
      }, [])}

      <button
        onClick={() => onPage(pageNumber + 1)}
        disabled={pageNumber === totalPages}
        className="p-2 rounded-lg text-slate-500 hover:bg-slate-100 disabled:opacity-40 disabled:cursor-not-allowed transition"
      >
        <ChevronRight className="h-4 w-4" />
      </button>
    </div>
  );
}