#!/usr/bin/env bash
# Runs EF Core migration commands for one module, or for every module, without typing the
# --project / --startup-project / --context arguments by hand.
#
#   scripts/migrations.sh add <module> <MigrationName>   create a migration after changing a module's entities
#   scripts/migrations.sh remove <module>                delete the module's last migration (only if not applied)
#   scripts/migrations.sh list <module>                  show the module's migrations and which are applied
#   scripts/migrations.sh update [module]                apply migrations to the database (all modules if omitted)
#
# <module> is one of the names in MODULES below. Needs the EF tool: dotnet tool install --global dotnet-ef
set -euo pipefail

cd "$(dirname "$0")/.."   # backend/, so the paths below work from any directory

STARTUP_PROJECT="src/CreditWorks.VehicleManagement.Api"

# module name -> "<project folder> <DbContext>". A new module needs one line here.
declare -A MODULES=(
  [categories]="src/Modules/CreditWorks.VehicleManagement.Modules.Categories CategoriesDbContext"
  [vehicles]="src/Modules/CreditWorks.VehicleManagement.Modules.Vehicles VehiclesDbContext"
)

# Apply modules in this order. Change it if a module's migrations ever depend on another's tables.
MODULE_ORDER=(categories vehicles)

usage() {
  sed -n '2,9p' "$0" | sed 's/^# \{0,1\}//'
  exit 1
}

# Runs one dotnet ef command against the given module.
run_ef() {
  local module="$1"; shift
  local entry="${MODULES[$module]:-}"
  if [[ -z "$entry" ]]; then
    echo "Unknown module '$module'. Choose one of: ${!MODULES[*]}" >&2
    exit 1
  fi

  local project context
  read -r project context <<< "$entry"
  echo "==> [$module] dotnet ef $*"
  dotnet ef "$@" --project "$project" --startup-project "$STARTUP_PROJECT" --context "$context"
}

if ! dotnet ef --version > /dev/null 2>&1; then
  echo "The EF Core tool is not installed. Run: dotnet tool install --global dotnet-ef" >&2
  exit 1
fi

command="${1:-}"
case "$command" in
  add)
    [[ $# -eq 3 ]] || usage
    run_ef "$2" migrations add "$3"
    ;;
  remove)
    [[ $# -eq 2 ]] || usage
    run_ef "$2" migrations remove
    ;;
  list)
    [[ $# -eq 2 ]] || usage
    run_ef "$2" migrations list
    ;;
  update)
    if [[ $# -eq 2 ]]; then
      run_ef "$2" database update
    elif [[ $# -eq 1 ]]; then
      for module in "${MODULE_ORDER[@]}"; do
        run_ef "$module" database update
      done
    else
      usage
    fi
    ;;
  *)
    usage
    ;;
esac
