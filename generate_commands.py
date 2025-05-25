import yaml
# todo :: lavoriamo su questa integrazione
with open("commands.yaml", "r") as f:
    commands = yaml.safe_load(f)

# Generate Python
with open("commands.py", "w") as py:
    py.write("# Auto-generated from commands.yaml\n")
    py.write("class Command:\n")
    for cmd in commands:
        py.write(f"    {cmd} = \"{cmd}\"\n")

# Generate C#
with open("Commands.cs", "w") as cs:
    cs.write("// Auto-generated from commands.yaml\n")
    cs.write("public static class Command {\n")
    for cmd in commands:
        cs.write(f"    public const string {cmd} = \"{cmd}\";\n")
    cs.write("}\n")
