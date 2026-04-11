# LIN File Format Specification

**Version:** 1.0  
**Project:** Bridge Master (.NET 10 / C#)  
**Source:** Reverse-engineered from BBO client behaviour, Forrest Rice's Bridgebots library, and BBO forum documentation.

> **Note:** BBO has never published an official LIN specification. This document consolidates community knowledge and is authoritative only for this project's parser and writer. Treat unexpected tag combinations as implementation-defined behaviour and handle them defensively.

---

## Table of Contents

1. [Overview](#1-overview)
2. [Syntax Rules](#2-syntax-rules)
3. [Tag Reference](#3-tag-reference)
4. [Deal Encoding](#4-deal-encoding)
5. [Bidding Encoding](#5-bidding-encoding)
6. [Play Encoding](#6-play-encoding)
7. [Teaching Tags](#7-teaching-tags)
8. [File Variants](#8-file-variants)
9. [Single-Board File Structure](#9-single-board-file-structure)
10. [Multi-Board File Structure](#10-multi-board-file-structure)
11. [HandViewer URL Format](#11-handviewer-url-format)
12. [Edge Cases and Gotchas](#12-edge-cases-and-gotchas)
13. [C# Implementation Notes](#13-c-implementation-notes)

---

## 1. Overview

LIN (origin of name unknown) is the proprietary file format used by Bridge Base Online (BBO) to record, replay, and teach bridge hands. It was originally created by Fred Gitelman as a private notation and was never intended to be a public standard.

LIN files power:
- The BBO Movie/replay system
- The BBO HandViewer web app
- The BBO Deal Archive
- Bridge Master interactive problems (the primary target of this project)

There are three distinct LIN flavours in the wild:
- **Classic** — produced by the BBO Windows client (NetBridgeVu)
- **Modern** — used by the HandViewer web app (`?lin=…` URL parameter)
- **Archive** — used for Deal Archive uploads

This spec covers all three where they differ.

---

## 2. Syntax Rules

### 2.1 Token Structure

A LIN file is a flat sequence of tokens. Each token is:

```
tag|value|
```

- `tag` — exactly two lowercase letters
- `value` — zero or more characters (may be empty)
- Tokens are separated by nothing; the closing `|` of one token is immediately followed by the opening tag of the next

### 2.2 Tokenisation Algorithm

```
remaining = full_lin_string
while remaining is not empty and not whitespace:
    (tag, value, remaining) = remaining.Split('|', maxCount: 3)
```

A single `Split('|', 3)` call on each iteration correctly handles values that contain no pipes.

### 2.3 Constraints

- The pipe character `|` **cannot appear** inside a value. There is no known escape mechanism.
- Tags are always exactly two characters. Unknown tags should be skipped without error.
- Whitespace (including newlines) between tokens is insignificant in single-board files but meaningful in multi-board files (boards are newline-separated).
- Values are case-sensitive for cards and suits (`S`, `H`, `D`, `C`) but BBO is tolerant of mixed case in practice.

---

## 3. Tag Reference

### 3.1 Header / Setup Tags

| Tag | Name | Value Format | Notes |
|-----|------|-------------|-------|
| `pn` | Player names | `South,West,North,East` | Comma-separated, always in S,W,N,E order regardless of dealer. In team files may be 8 names (open+closed room). |
| `st` | Start | *(empty)* | Resets board state. Always `st\|\|`. |
| `md` | Make deal | `#SOUTH,WEST,NORTH[,EAST]` | See §4. Required for every board. |
| `sv` | Vulnerability | `o` / `n` / `e` / `b` / `0` | `o`=none, `n`=NS, `e`=EW, `b`=both. `0` is an alias for none used in older files. |
| `ah` | Annotation header | `Board N` | Human-readable board label. |
| `rh` | Reset header | *(empty)* | Always `rh\|\|`. Purpose unclear; include for compatibility. |
| `qx` | Queue index | `o1`, `c1`, `o2,BOARD 2` | Board separator in multi-board files. Prefix `o`=open room, `c`=closed room. |
| `vg` | Vugraph / event | `Name,,I,16,16,,,,,` | Event metadata in team files. Only in file header. |
| `rs` | Results | `,,` or score strings | Score summary in team files. Only in file header. |

### 3.2 Bidding Tags

| Tag | Name | Value Format | Notes |
|-----|------|-------------|-------|
| `mb` | Make bid | `1N`, `p`, `d`, `r`, `2H!` | One bid per tag. Appending `!` marks bid as alerted. |
| `an` | Annotation | `15-17`, `spades` | Alert explanation. Belongs to the immediately preceding `mb`. Suit symbols: `!s !h !d !c` = ♠ ♥ ♦ ♣. |

### 3.3 Play Tags

| Tag | Name | Value Format | Notes |
|-----|------|-------------|-------|
| `pc` | Play card | `CA`, `HT`, `S9` | One card per tag. Format: suit letter + rank. |
| `pg` | Page break | *(empty)* | Separates tricks. Placed after every 4th `pc` tag, and after the final auction bid. Also used to pace display in the viewer. |
| `mc` | Make claim | `9` | Integer = tricks claimed by declarer. Ends play. |

### 3.4 Teaching Tags

| Tag | Name | Value Format | Notes |
|-----|------|-------------|-------|
| `up` | Undo play | `4`, `45` | Rewind N cards from current position. `up\|4\|` = one trick. Used before `pf\|y\|`. |
| `pf` | Play flag | `y` | Makes subsequent cards interactive. Used after `up`. |
| `nt` | Note | `Your turn to declare` | Display a text message to the user. Appears in both bidding and play contexts. |
| `sk` | Show/hide hands | `SN`, `EW`, `S` | Show only the listed directions' hands; hide the rest. |
| `ht` | Hand to highlight | `h` | Marks the human's hand in a teaching scenario. |

### 3.5 Scoring / Metadata Tags

| Tag | Name | Value Format | Notes |
|-----|------|-------------|-------|
| `em` | Expected margin | `NS 0.00` | IMP or matchpoint margin annotation. |
| `sa` | Score annotation | `0` | Numeric score. |

---

## 4. Deal Encoding

### 4.1 The `md` Tag

```
md|#SOUTH,WEST,NORTH[,EAST]|
```

- The first character is the **dealer digit**: `1`=South, `2`=West, `3`=North, `4`=East
- The remaining string is a comma-separated list of three or four holdings
- The **East hand may be omitted**; it is inferred as the remaining 13 cards
- Holdings are ordered **South, West, North, East** (not the usual N/S/E/W compass order)

> **Conversion from Direction enum:**  
> `lin_dealer = ((int)dealer + 2) % 4 + 1`  
> where South=0, West=1, North=2, East=3.

### 4.2 Holding Format

Each holding is a concatenation of four suit groups, ordered **S H D C** (high to low within each suit):

```
SAKQHJ9D54C32
```

- Each group starts with the suit letter (`S`, `H`, `D`, `C`)
- Followed by ranks in descending order: `A K Q J T 9 8 7 6 5 4 3 2`
- **All four suit letters must be present** even for voids: `SHJ9DCA` (void in spades)
- Ranks use `T` for ten

### 4.3 Vulnerability

| `sv` value | Meaning |
|-----------|---------|
| `o` | Neither vulnerable |
| `n` | NS vulnerable |
| `e` | EW vulnerable |
| `b` | Both vulnerable |
| `0` | Neither (legacy alias) |

### 4.4 Example

```
md|2SAK2HA95DAJ875C92,SQ765HKQ64D32CAK6,S9HJ872DQT64CQJ43,|
```

- Dealer: West (2)
- South: ♠AK2 ♥A95 ♦AJ875 ♣92
- West: ♠Q765 ♥KQ64 ♦32 ♣AK6
- North: ♠9 ♥J872 ♦QT64 ♣QJ43
- East: inferred

---

## 5. Bidding Encoding

### 5.1 Bid Tokens

Each bid is a separate `mb` tag:

```
mb|1N|mb|p|mb|2H|an|transfer|mb|p|mb|2S|mb|p|mb|p|mb|p|pg||
```

| Bid | LIN token |
|-----|----------|
| Pass | `p` |
| Double | `d` |
| Redouble | `r` |
| 1NT | `1N` (not `1NT`) |
| 2♥ | `2H` |
| 3♠ | `3S` |
| Alerted bid | `2H!` (append `!`) |

### 5.2 Alert Annotations

`an` always refers to the **immediately preceding** `mb`:

```
mb|2H!|an|spades, 5+|
```

Track `an` nodes by maintaining a counter of `mb` nodes seen so far and storing `(bidIndex, explanationText)`.

### 5.3 Auction Termination

The auction ends with `pg||` after the final `mb`. The contract is determined by working backwards through the bidding record:
- Find the last non-Pass bid
- If it is `X` or `XX`, continue backwards to find the contract denomination
- The declarer is determined from the play (first card's leader → declarer = leader.Previous())

### 5.4 Pass-Out

A fully passed-out hand is four consecutive `p` bids. Declarer is set to the dealer.

### 5.5 Compact Bidding Format (Classic only)

The BBO Windows client accepts a compact form:

```
mb|-1Dp1Sp2Hp3Hppp|
```

The HandViewer web app requires the verbose form (one bid per `mb` tag). **This project targets the Modern/Archive format; compact form support is optional.**

---

## 6. Play Encoding

### 6.1 Card Tokens

```
pc|CA|pc|C5|pc|C4|pc|C2|pg||
```

- One card per `pc` tag
- Format: suit letter + rank (`CA`=♣A, `HT`=♥10, `S9`=♠9)
- Four consecutive `pc` tags form one trick
- `pg||` follows each complete trick

### 6.2 Trick Order

Cards within a trick are in clockwise order starting from the **lead**. Given the lead direction, the four cards map to: lead, lead+1, lead+2, lead+3 (clockwise).

The first `pc` of the hand is always led by the player to declarer's left (i.e. `declarer.Next()`).

### 6.3 Claims

```
pc|SA|mc|13|pg||
```

`mc` may appear mid-trick or after a complete trick. Its value is the **total tricks taken by declarer** (not the remaining tricks). Play ends at this point.

### 6.4 Determining Declarer

From the play record:
1. Identify the first card played
2. Find which Direction holds that card in the deal → this is the **opening leader**
3. Declarer = `leader.Previous()` (the player to leader's right)

This is more reliable than inferring from the auction alone (handles unusual contracts).

### 6.5 Trick Evaluation (when no `mc`)

If 52 cards are played without a claim, evaluate each trick:
- Trump suit from contract level indicator
- Within a trick, highest card of led suit wins, unless a trump is played (highest trump wins)
- Track leader direction through the hand

---

## 7. Teaching Tags

These tags are used in Bridge Master-style interactive files and are **not** generated by standard BBO gameplay recording.

### 7.1 Workflow

```
mb|...|pg||           ← auction
pc|CK|pc|C2|pc|C5|pc|C3|pg||   ← demonstrate one trick
up|4|                 ← rewind that trick
pf|y|                 ← make position interactive
nt|Now find the best line|   ← prompt text
```

### 7.2 `up|N|` — Undo Play

Rewinds `N` cards from the current play position. Common values:
- `up|4|` — undo one trick
- `up|8|` — undo two tricks
- `up|45|` — rewind to 7 cards before the end of a 52-card sequence (used when 7 cards were played before a claim)

### 7.3 `pf|y|` — Play Flag

Enables interactive card play from the current position. The viewer accepts user input for subsequent cards.

### 7.4 `sk|XY|` — Show/Hide Hands

Controls hand visibility. The value is a string of direction letters to **show**:

| Value | Effect |
|-------|--------|
| `SN` | Show South and North; hide East and West |
| `EW` | Show East and West; hide South and North |
| `S`  | Show South only |
| *(empty)* | Show all hands |

### 7.5 `nt|text|` — Note

Displays a message. May appear in the bidding sequence or play sequence. Track context:
- If any `pc` tags have been seen: `play_index = count(pc) - 1`
- Otherwise: `bid_index = count(mb) - 1`

### 7.6 `ht|h|` — Hand to Highlight

Marks the human player's seat. Used in conjunction with `sk` to present declarer-play or defender problems.

---

## 8. File Variants

### 8.1 Single-Board (board-per-line)

- Each board is a single line of LIN tokens
- Multiple boards = multiple lines
- Header tags (`pn`, `st`) repeat on each line
- Produced by the HandViewer URL format

### 8.2 Multi-Board (session file)

- A file-level header block (containing `vg`, `rs`, `pn`) precedes the boards
- The header ends with `pg||`
- Each subsequent board begins with a `qx|oN|` token
- Boards are separated by newlines
- Player names in the header apply to all boards (do not repeat per board)

### 8.3 Team (Vugraph) Files

- Two sets of four player names in `pn` (8 total): `S1,W1,N1,E1,S2,W2,N2,E2`
- `qx|o1|` = open room board 1, `qx|c1|` = closed room board 1
- The `rs` tag contains a result string
- Use `qx` prefix to select which player name set applies

---

## 9. Single-Board File Structure

Minimum valid single-board LIN:

```
pn|South,West,North,East|st||md|3SKQ65H63DJ74C7642,STHQ95DQT8532CA85,SAJ984HAJ74DAKCJ3,|rh||ah|Board 1|sv|o|mb|1S|mb|p|mb|4S|mb|p|mb|p|mb|p|pg||pc|CK|pc|C2|pc|CA|pc|C3|pg||mc|10|pg||
```

### 9.1 Recommended Order

```
pn|...|           Player names
st||              Start/reset
md|#...|          Deal (required)
rh||              Reset header
ah|Board N|       Board label
sv|x|             Vulnerability
mb|...|           Bidding (one per tag)
[an|...|]         Alert explanations (after alerted mb)
pg||              End of auction
pc|...|           Play cards (one per tag)
pg||              After each complete trick
[mc|N|]           Claim (optional)
pg||              End of board
```

---

## 10. Multi-Board File Structure

```
vg|Event Name,,I,16,16,,,,,|rs|,,|pn|S1,W1,N1,E1,S2,W2,N2,E2|pg||
qx|o1|md|3...|rh||ah|Board 1|sv|o|mb|...|pg||pc|...|pg||
qx|c1|md|3...|rh||ah|Board 1|sv|o|mb|...|pg||pc|...|pg||
qx|o2|md|4...|rh||ah|Board 2|sv|n|mb|...|pg||pc|...|pg||
qx|c2|md|4...|rh||ah|Board 2|sv|n|mb|...|pg||pc|...|pg||
```

### 10.1 Header Parsing

The file header ends at the first `pg||`. Read lines into a buffer until `pg||` is encountered (stripping newlines), then parse as a single LIN token sequence.

### 10.2 Board Parsing

After the header, each line starting with `qx` begins a new board. Concatenate continuation lines (lines not starting with `qx`) into the current board string before parsing.

---

## 11. HandViewer URL Format

The HandViewer supports an alternative URL parameter format that is **not** the same as inline LIN:

```
https://www.bridgebase.com/tools/handviewer.html
  ?n=SKHA...        North hand (full holding, no suit separators)
  &s=...            South hand
  &e=...            East hand
  &w=...            West hand
  &d=N              Dealer (N/S/E/W)
  &v=b              Vulnerability (o/n/e/b)
  &b=7              Board number
  &a=PP1S...        Compact auction string
  &p=CQC6...        Compact play string (2 chars per card)
  &nn=Meckstroth    North player name
  &sn=Rodwell       South player name
  &en=Mari          East player name
  &wn=Levy          West player name
```

The `lin=` parameter takes a full LIN token string (URL-encoded). The `linurl=` parameter takes a URL pointing to an XML file wrapping a LIN string.

### 11.1 Compact Auction String

Bids are concatenated without delimiters: `PP1S2HP3HP3SP4DP4HPPP`

Parsing algorithm: iterate character by character; accumulate into current bid; flush when:
- A letter follows a digit (e.g. `1S` → `1` then `S` ends the bid)
- `P` is a single-char pass
- `D` is double, `R` is redouble (when not preceded by a digit)

### 11.2 Compact Play String

Two characters per card, concatenated: `CQCAK5...`
Parse as `play[i..i+2]` for `i` in steps of 2.

---

## 12. Edge Cases and Gotchas

### 12.1 Dealer Encoding

BBO's dealer digit uses a non-standard direction order:

| Digit | Direction |
|-------|-----------|
| `1` | South |
| `2` | West |
| `3` | North |
| `4` | East |

Conversion: `linDealer = ((int)dealer + 2) % 4 + 1` (South=0, West=1, North=2, East=3)

### 12.2 Eight Player Names in Team Files

```
pn|S_open,W_open,N_open,E_open,S_closed,W_closed,N_closed,E_closed|
```

When `pn` contains 8 names, use names 0–3 for `qx|o...|` boards and names 4–7 for `qx|c...|` boards. Exception: if names 4–7 are the placeholder strings `"South","West","North","East"`, use names 0–3 for all boards.

### 12.3 Vulnerability Aliases

`sv|0|` means the same as `sv|o|` (neither vulnerable). Both must be handled.

### 12.4 Alerted vs Explained Bids

- `!` suffix on a bid value = alerted (partner's explanation will follow)
- `an` following a non-`!` bid = explanation/announcement without alert flag
- Both cases should produce a `BidAnnotation` record; only `!` sets `IsAlerted = true`

### 12.5 NT vs N

In LIN, notrump is abbreviated `N` not `NT`. `1N`, `2N`, `3N`, `4N`, `6N`, `7N`. When writing, always use `N`; when reading, accept both `N` and `NT`.

### 12.6 Ten

Ten is always `T`, never `10`.

### 12.7 Missing East Hand

The East hand in `md` is frequently omitted. When parsing, if only three holdings are present after the dealer digit, infer East's hand as the 13 cards not held by the other three players.

### 12.8 `pg||` Placement

`pg||` after the auction and after each trick is required for HandViewer compatibility. When writing LIN, always emit `pg||` at these points. When reading, `pg||` tokens may be safely ignored (they carry no state information for parsing).

### 12.9 `nt|` Context

A `nt` token's meaning depends on what has been seen so far in the board:
- If `pc` count > 0: the note is associated with the play at `playIndex = pcCount - 1`
- Otherwise: associated with the auction at `bidIndex = mbCount - 1`

### 12.10 `mc` Mid-Trick

A claim may occur after 1, 2, or 3 cards of a trick (not just after a full trick). Do not assume `mc` only appears at trick boundaries.

---

## 13. C# Implementation Notes

### 13.1 Recommended Parsing Entry Point

```csharp
// Tokenise
static IEnumerable<(string Tag, string Value)> Tokenise(string lin)
{
    var span = lin.AsSpan().TrimStart();
    while (!span.IsEmpty)
    {
        int firstPipe = span.IndexOf('|');
        if (firstPipe < 0) yield break;
        var tag = span[..firstPipe].ToString();
        span = span[(firstPipe + 1)..];
        int secondPipe = span.IndexOf('|');
        if (secondPipe < 0) yield break;
        var value = span[..secondPipe].ToString();
        span = span[(secondPipe + 1)..].TrimStart();
        yield return (tag, value);
    }
}
```

### 13.2 Dealer Conversion

```csharp
// BBO dealer digit → Direction
Direction DealerFromLin(char c) => c switch
{
    '1' => Direction.South,
    '2' => Direction.West,
    '3' => Direction.North,
    '4' => Direction.East,
    _   => throw new FormatException($"Invalid LIN dealer: {c}")
};

// Direction → BBO dealer digit
char DealerToLin(Direction d) => (char)('0' + ((int)d + 2) % 4 + 1);
```

### 13.3 Vulnerability Parsing

```csharp
(bool ns, bool ew) ParseVulnerability(string sv) => sv.ToLowerInvariant() switch
{
    "n"       => (true, false),
    "e"       => (false, true),
    "b"       => (true, true),
    "o" or "0" or "" => (false, false),
    _ => throw new FormatException($"Unknown vulnerability: {sv}")
};
```

### 13.4 Card Parsing

```csharp
// "CA" → (Suit.Clubs, Rank.Ace)
(Suit, Rank) ParseCard(string s)
{
    var suit = s[0] switch
    {
        'S' => Suit.Spades, 'H' => Suit.Hearts,
        'D' => Suit.Diamonds, 'C' => Suit.Clubs,
        _ => throw new FormatException($"Unknown suit: {s[0]}")
    };
    var rank = s[1] switch
    {
        'A' => Rank.Ace,   'K' => Rank.King,  'Q' => Rank.Queen,
        'J' => Rank.Jack,  'T' => Rank.Ten,   '9' => Rank.Nine,
        '8' => Rank.Eight, '7' => Rank.Seven, '6' => Rank.Six,
        '5' => Rank.Five,  '4' => Rank.Four,  '3' => Rank.Three,
        '2' => Rank.Two,
        _ => throw new FormatException($"Unknown rank: {s[1]}")
    };
    return (suit, rank);
}
```

### 13.5 Multi-Board Header Detection

```csharp
bool IsMultiLin(string content) =>
    content.TrimStart().StartsWith("vg|") ||
    content.TrimStart().StartsWith("rs|") ||
    content.Contains("\nqx|");
```

### 13.6 Key Types to Define

```
Direction       : enum { South, West, North, East }
Suit            : enum { Spades, Hearts, Diamonds, Clubs }
Rank            : enum { Two..Ace }
Card            : (Suit, Rank)
PlayerHand      : Dictionary<Suit, IReadOnlyList<Rank>>
Deal            : Dealer, NsVulnerable, EwVulnerable, Hands[Direction]
BidAnnotation   : BidIndex, Bid, IsAlerted, Explanation
Commentary      : BidIndex?, PlayIndex?, Text
BoardRecord     : Bids, BidAnnotations, Play, Declarer, Contract, Tricks, Names, Commentary
DealRecord      : Deal, BoardRecords[]
```

### 13.7 Teaching State Machine

For Bridge Master, maintain a `TeachingState` alongside the board:

```
PlayPosition    : current card index in the play record
IsInteractive   : set by pf|y|, cleared on next board
HiddenHands     : set by sk|XX|
HighlightedHand : set by ht|X|
PendingNote     : set by nt|...|
```

Process `up|N|` by decrementing `PlayPosition` by N, then apply `pf|y|` and `nt` to the resulting position.

---

## Appendix A: Quick Reference

```
pn|S,W,N,E|        player names
st||               start/reset
md|#S,W,N[,E]|     deal (# = dealer digit 1-4: S W N E)
sv|o/n/e/b|        vulnerability
rh||               reset header
ah|Board N|        board label
qx|o1|             board separator (multi-board)
vg|Name,,I,…|      event header (team files)
rs|…|              results (team files)
mb|bid|            one bid (p=pass d=dbl r=rdbl, append ! for alert)
an|text|           alert explanation (follows mb)
pg||               page break (end of auction, end of each trick)
pc|XY|             play one card (X=suit, Y=rank, T=ten)
mc|N|              claim N tricks for declarer
up|N|              rewind N cards
pf|y|              enable interactive play
nt|text|           display note
sk|SN|             show/hide hands
ht|h|              highlight human's hand
```

## Appendix B: Suit Symbols in Text

Annotations and notes use `!s !h !d !c` as suit symbols:

| LIN | Symbol |
|-----|--------|
| `!s` | ♠ |
| `!h` | ♥ |
| `!d` | ♦ |
| `!c` | ♣ |

These appear in `an|` and `nt|` values and should be rendered as the corresponding Unicode symbols in the UI.

## Appendix C: References

- Forrest Rice, [Bridgebots lin.py](https://github.com/forrestrice/bridge-bots/blob/master/bridgebots/bridgebots/lin.py) — Reference Python implementation
- Forrest Rice, [Introducing Bridgebots Part 3](https://forrestrice.com/posts/Introducing-Bridgebots-Part-3/) — LIN format walkthrough
- BBO Forums, [Lin file format](https://www.bridgebase.com/forums/topic/85366-lin-file-format/) — Community documentation
- BBO Forums, [Lin files format (2003)](https://www.bridgebase.com/forums/topic/791-lin-files-format/) — Original community reverse-engineering thread
- BBO, [HandViewer documentation](https://www.bridgebase.com/tools/hvdoc.html) — URL parameter format
