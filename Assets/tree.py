import os

def print_tree(path, prefix=""):
    try:
        entries = sorted(os.listdir(path))
    except PermissionError:
        print(prefix + "⛔ Zugriff verweigert")
        return

    entries_count = len(entries)

    for index, name in enumerate(entries):
        full_path = os.path.join(path, name)

        is_last = (index == entries_count - 1)
        connector = "└── " if is_last else "├── "
        print(prefix + connector + name)

        if os.path.isdir(full_path):
            extension = "    " if is_last else "│   "
            print_tree(full_path, prefix + extension)


if __name__ == "__main__":
    folder = input("Pfad zum Ordner eingeben: ").strip()
    if os.path.exists(folder):
        print("\n📁 Ordnerstruktur:\n")
        print_tree(folder)
    else:
        print("❌ Pfad existiert nicht.")

