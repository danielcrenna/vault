using ebnf;

namespace ripple
{
    /*
    function EARLEY-PARSE(words, grammar)
    ENQUEUE((γ → •S, 0), chart[0])
    for i ← from 0 to LENGTH(words) do
        for each state in chart[i] do
            if INCOMPLETE?(state) then
                if NEXT-CAT(state) is a nonterminal then
                    PREDICTOR(state, i, grammar)         // non-terminal
                else do
                    SCANNER(state, i)                    // terminal
            else do
                COMPLETER(state, i)
        end
    end
    return chart
 
    procedure PREDICTOR((A → α•B, i), j, grammar),
        for each (B → γ) in GRAMMAR-RULES-FOR(B, grammar) do
            ADD-TO-SET((B → •γ, j), chart[ j])
        end
 
    procedure SCANNER((A → α•B, i), j),
        if B ⊂ PARTS-OF-SPEECH(word[j]) then
            ADD-TO-SET((B → word[j], i), chart[j + 1])
        end
 
    procedure COMPLETER((B → γ•, j), k),
        for each (A → α•Bβ, i) in chart[j] do
            ADD-TO-SET((A → αB•β, i), chart[k])
        end
    */
    public partial class Earley : Generator
    {
        private readonly Grammar _grammar;
        public Earley(Grammar grammar)
        {
            _grammar = grammar;
        }
    }
}
