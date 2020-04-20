//
//	Regular Expression Engine C# Sample Application
//	2006, by Leniel Braz de Oliveira Maccaferri & Wellington Magalhães Leite.
//  https://www.leniel.net/2009/02/regular-expression-engine-in-csharp.html
//
//  UBM's Computer Engineering - 7th term [http://www.ubm.br/]
//  
//  This program sample was developed and turned in as a term paper for Lab. of
//  Compilers Construction. It was based on the source code provided by Eli Bendersky
//  [http://eli.thegreenplace.net/] and is provided "as is" without warranty.
//
//  It makes use of C5 library.
//

using System;
using SCG = System.Collections.Generic;
using C5;

using state = System.Int32;
using input = System.Char;

namespace RegularExpressionEngine
{
  class SubsetMachine
  {
    private static int num = 0;

    /// <summary>
    /// Subset machine that employs the powerset construction or subset construction algorithm.
    /// It creates a DFA that recognizes the same language as the given NFA.
    /// </summary>
    public static DFA SubsetConstruct(NFA nfa)
    {
      DFA dfa = new DFA();

      // Sets of NFA states which is represented by some DFA state
      Set<Set<state>> markedStates = new Set<Set<state>>();
      Set<Set<state>> unmarkedStates = new Set<Set<state>>();

      // Gives a number to each state in the DFA
      HashDictionary<Set<state>, state> dfaStateNum = new HashDictionary<Set<state>, state>();

      Set<state> nfaInitial = new Set<state>();
      nfaInitial.Add(nfa.initial);

      // Initially, EpsilonClosure(nfa.initial) is the only state in the DFAs states
      // and it's unmarked.
      Set<state> first = EpsilonClosure(nfa, nfaInitial);
      unmarkedStates.Add(first);

      // The initial dfa state
      state dfaInitial = GenNewState();
      dfaStateNum[first] = dfaInitial;
      dfa.start = dfaInitial;

      while(unmarkedStates.Count != 0)
      {
        // Takes out one unmarked state and posteriorly mark it.
        Set<state> aState = unmarkedStates.Choose();

        // Removes from the unmarked set.
        unmarkedStates.Remove(aState);

        // Inserts into the marked set.
        markedStates.Add(aState);

        // If this state contains the NFA's final state, add it to the DFA's set of
        // final states.
        if(aState.Contains(nfa.final))
          dfa.final.Add(dfaStateNum[aState]);

        SCG.IEnumerator<input> iE = nfa.inputs.GetEnumerator();

        // For each input symbol the NFA knows...
        while(iE.MoveNext())
        {
          // Next state
          Set<state> next = EpsilonClosure(nfa, nfa.Move(aState, iE.Current));

          // If we haven't examined this state before, add it to the unmarkedStates,
          // and make up a new number for it.
          if(!unmarkedStates.Contains(next) && !markedStates.Contains(next))
          {
            unmarkedStates.Add(next);
            dfaStateNum.Add(next, GenNewState());
          }

          KeyValuePair<state, input> transition = new KeyValuePair<state, input>();
          transition.Key = dfaStateNum[aState];
          transition.Value = iE.Current;

          dfa.transTable[transition] = dfaStateNum[next];
        }
      }

      return dfa;
    }

    /// <summary>
    /// Builds the Epsilon closure of states for the given NFA 
    /// </summary>
    /// <param name="nfa"></param>
    /// <param name="states"></param>
    /// <returns></returns>
    static Set<state> EpsilonClosure(NFA nfa, Set<state> states)
    {
      // Push all states onto a stack
      SCG.Stack<state> uncheckedStack = new SCG.Stack<state>(states);

      // Initialize EpsilonClosure(states) to states
      Set<state> epsilonClosure = states;

      while(uncheckedStack.Count != 0)
      {
        // Pop state t, the top element, off the stack
        state t = uncheckedStack.Pop();

        int i = 0;

        // For each state u with an edge from t to u labeled Epsilon
        foreach(input input in nfa.transTable[t])
        {
          if(input == (char)NFA.Constants.Epsilon)
          {
            state u = Array.IndexOf(nfa.transTable[t], input, i);

            // If u is not already in epsilonClosure, add it and push it onto stack
            if(!epsilonClosure.Contains(u))
            {
              epsilonClosure.Add(u);
              uncheckedStack.Push(u);
            }
          }

          i = i + 1;
        }
      }

      return epsilonClosure;
    }

    /// <summary>
    /// Creates unique state numbers for DFA states
    /// </summary>
    /// <returns></returns>
    private static state GenNewState()
    {
      return num++;
    }

  }
}