// src/components/cms/ReorderableTable.jsx
import { useEffect, useState } from 'react';
import {
  DndContext, closestCenter, PointerSensor, KeyboardSensor, useSensor, useSensors,
} from '@dnd-kit/core';
import {
  SortableContext, verticalListSortingStrategy, useSortable,
  sortableKeyboardCoordinates, arrayMove,
} from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { GripVertical, ChevronUp, ChevronDown, Check, X } from 'lucide-react';
import Spinner from '@/components/ui/Spinner';
import Button from '@/components/ui/Button';

function SortableRow({ id, index, total, onMove, children }) {
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } =
    useSortable({ id });
  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0.6 : 1,
  };
  return (
    <tr ref={setNodeRef} style={style} className="bg-white hover:bg-slate-50 transition-colors">
      <td className="pl-3 pr-1 py-3 w-14">
        <div className="flex items-center gap-1">
          <button type="button" {...attributes} {...listeners}
            className="cursor-grab active:cursor-grabbing text-slate-300 hover:text-slate-500 touch-none">
            <GripVertical className="h-4 w-4" />
          </button>
          <div className="flex flex-col">
            <button type="button" disabled={index === 0} onClick={() => onMove(index, -1)}
              className="text-slate-300 hover:text-brand-600 disabled:opacity-30 disabled:hover:text-slate-300 transition">
              <ChevronUp className="h-3.5 w-3.5" />
            </button>
            <button type="button" disabled={index === total - 1} onClick={() => onMove(index, 1)}
              className="text-slate-300 hover:text-brand-600 disabled:opacity-30 disabled:hover:text-slate-300 transition">
              <ChevronDown className="h-3.5 w-3.5" />
            </button>
          </div>
        </div>
      </td>
      {children}
    </tr>
  );
}

export default function ReorderableTable({
  columns, data, loading, orderField = 'displayOrder', onSaveOrder,
  emptyMessage = 'No data found.',
}) {
  const [ordered, setOrdered] = useState([]);
  const [dirty, setDirty]     = useState(false);
  const [saving, setSaving]   = useState(false);

  useEffect(() => {
    const sorted = [...data].sort((a, b) => (a[orderField] ?? 0) - (b[orderField] ?? 0));
    setOrdered(sorted);
    setDirty(false);
  }, [data, orderField]);

  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 5 } }),
    useSensor(KeyboardSensor, { coordinateGetter: sortableKeyboardCoordinates }),
  );

  const handleDragEnd = ({ active, over }) => {
    if (!over || active.id === over.id) return;
    setOrdered((items) => {
      const oldIndex = items.findIndex((i) => i.id === active.id);
      const newIndex = items.findIndex((i) => i.id === over.id);
      return arrayMove(items, oldIndex, newIndex);
    });
    setDirty(true);
  };

  const moveRow = (index, dir) => {
    const newIndex = index + dir;
    if (newIndex < 0 || newIndex >= ordered.length) return;
    setOrdered((items) => arrayMove(items, index, newIndex));
    setDirty(true);
  };

  const handleSave = async () => {
    setSaving(true);
    try {
      await onSaveOrder(ordered);
      setDirty(false);
    } finally {
      setSaving(false);
    }
  };

  const handleCancel = () => {
    setOrdered([...data].sort((a, b) => (a[orderField] ?? 0) - (b[orderField] ?? 0)));
    setDirty(false);
  };

  return (
    <div>
      {dirty && (
        <div className="flex items-center justify-between bg-brand-50 border border-brand-100 rounded-xl px-4 py-2.5 mb-3">
          <p className="text-sm text-brand-700">Order changed — save to apply.</p>
          <div className="flex gap-2">
            <Button variant="secondary" size="sm" onClick={handleCancel}>
              <X className="h-3.5 w-3.5" /> Cancel
            </Button>
            <Button size="sm" loading={saving} onClick={handleSave}>
              <Check className="h-3.5 w-3.5" /> Save Order
            </Button>
          </div>
        </div>
      )}

      <div className="overflow-x-auto rounded-2xl border border-slate-100">
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-slate-50 border-b border-slate-100">
              <th className="w-14" />
              {columns.map((col) => (
                <th key={col.key}
                  className="px-4 py-3 text-left text-xs font-semibold text-slate-500 uppercase tracking-wider whitespace-nowrap">
                  {col.header}
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-50">
            {loading ? (
              <tr><td colSpan={columns.length + 1} className="py-16 text-center"><Spinner className="mx-auto" /></td></tr>
            ) : ordered.length === 0 ? (
              <tr><td colSpan={columns.length + 1} className="py-16 text-center text-slate-400">{emptyMessage}</td></tr>
            ) : (
              <DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={handleDragEnd}>
                <SortableContext items={ordered.map((i) => i.id)} strategy={verticalListSortingStrategy}>
                  {ordered.map((row, index) => (
                    <SortableRow key={row.id} id={row.id} index={index} total={ordered.length} onMove={moveRow}>
                      {columns.map((col) => (
                        <td key={col.key} className="px-4 py-3 text-slate-700 whitespace-nowrap">
                          {col.render ? col.render(row) : row[col.key] ?? '—'}
                        </td>
                      ))}
                    </SortableRow>
                  ))}
                </SortableContext>
              </DndContext>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}