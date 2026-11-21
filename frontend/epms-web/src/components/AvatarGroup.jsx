import Avatar from './Avatar';

function AvatarGroup({ people = [], max = 4 }) {
  if (people.length === 0) {
    return <span className="text-secondary small">Unassigned</span>;
  }

  const visible = people.slice(0, max);
  const remaining = people.length - visible.length;

  return (
    <div className="avatar-group d-flex align-items-center">
      {visible.map((person) => (
        <Avatar key={person.id} name={person.name} seed={person.id} size="sm" />
      ))}
      {remaining > 0 && (
        <span className="avatar avatar-sm avatar-more" title={people.slice(max).map((person) => person.name).join(', ')}>
          +{remaining}
        </span>
      )}
    </div>
  );
}

export default AvatarGroup;
