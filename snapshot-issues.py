#!/usr/bin/env python3
import subprocess, json, sys, textwrap, requests, os

# --- CONFIGURA QUI ---
GH_TOKEN    = GH_TOKEN = os.getenv("GITHUB_TOKEN") or os.getenv("GH_TOKEN")
GH_ORG       = "NeoNix-Lab"
GH_PROJECT  = 4               # numero del Project v2
GH_REPOS    = ["NeoQuantowerTools","Rnn_V0_1", "QT_Ai_Plug_In_Integration"]    # lista dei repo
OUT_FILE    = "issues.json"
# ----------------------

headers = {
    "Authorization": f"bearer {GH_TOKEN}",
    "Accept": "application/vnd.github.v3+json"
}

def fetch_project_issues():
    query = (
        'query($org:String!,$num:Int!){'
        'user(login:$org){'
        'projectV2(number:$num){'
        'items(first:100){nodes{content{__typename '
        '... on Issue{repository{name} number title body '
        'labels(first:10){nodes{name}} '
        'assignees(first:10){nodes{login}} '
        'state}}}}}}}'
    )
    resp = requests.post(
        "https://api.github.com/graphql",
        json={"query": query, "variables": {"org": GH_ORG, "num": GH_PROJECT}},
        headers=headers
    )
    resp.raise_for_status()
    

    data = resp.json()["data"]["user"]["projectV2"]["items"]["nodes"]
    out = []
    for n in data:
        c = n["content"]
        if c.get("__typename") != "Issue": continue
        out.append({
            "repo":      c["repository"]["name"],
            "number":    c["number"],
            "title":     c["title"],
            "body":      c["body"] or "",
            "labels":    [l["name"] for l in c["labels"]["nodes"]],
            "assignees":[a["login"] for a in c["assignees"]["nodes"]],
            "state":     c["state"].lower(),
            "source":    "project"
        })
    return out

def fetch_repo_issues():
    out = []
    for repo in GH_REPOS:
        url = f"https://api.github.com/repos/{GH_ORG}/{repo}/issues"
        params = {"state":"open","per_page":100}
        page = 1
        while True:
            resp = requests.get(url, headers=headers, params={**params, "page": page})
            resp.raise_for_status()
            items = resp.json()
            if not items: break
            for i in items:
                # filtra le pull request
                if "pull_request" in i: continue
                out.append({
                    "repo":      repo,
                    "number":    i["number"],
                    "title":     i["title"],
                    "body":      i["body"] or "",
                    "labels":    [l["name"] for l in i["labels"]],
                    "assignees":[a["login"] for a in i["assignees"]],
                    "state":     i["state"],
                    "source":    "repo"
                })
            page += 1
    return out

def main():
    print("🚀 Fetching issues from project…")
    project_issues = fetch_project_issues()
    print(f"  → {len(project_issues)} item dal project")
    print("🚀 Fetching issues from repos…")
    repo_issues = fetch_repo_issues()
    print(f"  → {len(repo_issues)} issue aperti nei repo")
    all_issues = project_issues #+ repo_issues
    with open(OUT_FILE, "w", encoding="utf-8") as f:
        json.dump(all_issues, f, indent=2, ensure_ascii=False)
    print(f"\n✅ Snapshot completo: {len(all_issues)} issue in {OUT_FILE}")

if __name__ == "__main__":
    main()
