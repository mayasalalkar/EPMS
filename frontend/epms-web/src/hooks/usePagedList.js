import { useEffect, useState } from 'react';
import { getErrorMessage } from '../utils/errors';
import useDebounce from './useDebounce';

export default function usePagedList(load, defaults = {}) {
  const [query, setQuery] = useState({ page: 1, pageSize: 10, sortDir: 'asc', ...defaults });
  const [search, setSearch] = useState('');
  const term = useDebounce(search);
  const [revision, setRevision] = useState(0);
  const [result, setResult] = useState(null);
  const loading = result?.query !== query || result?.term !== term || result?.revision !== revision;

  useEffect(() => {
    let active = true;
    load({ ...query, search: term })
      .then((data) => {
        if (!active) return;
        if (query.page > 1 && data.items.length === 0) {
          setQuery((current) => ({ ...current, page: Math.max(1, data.totalPages) }));
        } else {
          setResult({ query, term, revision, data, error: '' });
        }
      })
      .catch((error) => {
        if (active) setResult({ query, term, revision, data: null, error: getErrorMessage(error) });
      });
    return () => { active = false; };
  }, [load, query, term, revision]);

  const update = (patch) => setQuery((current) => ({ ...current, page: 1, ...patch }));
  const sort = (field) => update({ sortBy: field, sortDir: query.sortBy === field && query.sortDir === 'asc' ? 'desc' : 'asc' });

  return {
    data: loading ? null : result.data, error: loading ? '' : result.error, loading, query, search, update, sort,
    changeSearch: (value) => { setSearch(value); if (query.page !== 1) update({}); },
    refresh: () => setRevision((value) => value + 1),
  };
}
