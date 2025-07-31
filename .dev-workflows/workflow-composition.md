# 🔗 Workflow Composition & Chaining Guide

## Overview
How to combine multiple workflows systematically, using outputs from one workflow as inputs to another.

## 🔄 Basic Workflow Chaining

### Sequential Execution
```
"Execute these workflows in sequence:
1. architecture-workflow.md phases 1-3 → Generate system design
2. feature-development.md phases 3-6 → Implement based on design  
3. testing-workflow.md → Validate implementation"
```

### Parallel + Merge
```
"Run these workflows in parallel, then merge:
1. ui-ux-workflow.md → Design interface
2. architecture-workflow.md → Design backend
3. Merge: feature-development.md → Integrate both designs"
```

## 📋 Common Workflow Combinations

### **Analysis → Planning → Implementation**
```
WORKFLOW CHAIN: "Complex Feature Development"

1. architecture-workflow.md (Phases 1-2)
   OUTPUT: System requirements, constraints analysis
   
2. ui-ux-workflow.md (Phases 1-3)  
   INPUT: Requirements from step 1
   OUTPUT: UI mockups, design system
   
3. feature-development.md (All phases)
   INPUT: Architecture + UI designs from steps 1-2
   OUTPUT: Complete feature implementation
```

### **Investigation → Fix → Validation**
```
WORKFLOW CHAIN: "Complex Bug Resolution"

1. bug-fix-workflow.md (Phases 1-2)
   OUTPUT: Root cause analysis, impact assessment
   
2. architecture-workflow.md (If architectural issue)
   INPUT: Root cause analysis from step 1
   OUTPUT: Structural fix design
   
3. feature-development.md (Implementation phases)
   INPUT: Fix design from step 2
   OUTPUT: Implemented solution
   
4. testing-workflow.md
   INPUT: Solution from step 3
   OUTPUT: Validated, tested fix
```

### **Design → Prototype → Refine → Scale**
```
WORKFLOW CHAIN: "UI/UX Overhaul"

1. ui-ux-workflow.md (Phases 1-2)
   OUTPUT: Initial design concepts
   
2. quick-templates.md (COMPONENT EXTRACTION)
   INPUT: Design concepts from step 1
   OUTPUT: Prototype components
   
3. testing-workflow.md (Usability testing)
   INPUT: Prototypes from step 2
   OUTPUT: User feedback and improvements
   
4. ui-ux-workflow.md (Phases 4-6)
   INPUT: Feedback from step 3
   OUTPUT: Final refined implementation
```

## 🎯 Workflow Input/Output Patterns

### **Architecture → Feature Development**
```
"Use architecture-workflow.md to design the authentication system, then use those architectural decisions as input for feature-development.md implementation"

HANDOFF ARTIFACTS:
- System architecture diagrams
- Component interface definitions  
- Data flow specifications
- Technology stack decisions
```

### **Bug Analysis → Feature Enhancement**
```
"Apply bug-fix-workflow.md to analyze the performance issue, then use those findings to drive feature-development.md for performance improvements"

HANDOFF ARTIFACTS:
- Root cause analysis report
- Performance bottleneck identification
- Improvement opportunities list
- Technical debt documentation
```

### **UI Research → Implementation → Testing**
```
"Chain ui-ux-workflow.md design phase → feature-development.md implementation → testing-workflow.md validation for the new dashboard"

HANDOFF ARTIFACTS:
- User research findings
- Wireframes and mockups
- Design system updates
- Component specifications
```

## 🔧 Workflow State Management

### **Checkpoint Pattern**
```
"Execute workflow A to checkpoint, review results, then proceed with workflow B based on outcomes"

EXAMPLE:
1. architecture-workflow.md phases 1-3 → CHECKPOINT
2. Review architectural decisions
3. feature-development.md phases 3-6 → Continue with implementation
```

### **Conditional Branching**
```
"Use workflow A to analyze, then branch based on results:
- If simple fix: quick-templates.md  
- If complex issue: architecture-workflow.md + feature-development.md"
```

### **Iterative Refinement**
```
"Cycle between ui-ux-workflow.md and testing-workflow.md until user acceptance criteria are met"

LOOP:
1. ui-ux-workflow.md → Design iteration
2. testing-workflow.md → User testing  
3. Evaluate results → Continue or iterate
```

## 📊 Advanced Composition Patterns

### **Multi-Stream Development**
```
PARALLEL STREAMS:
Stream A: architecture-workflow.md → Backend design
Stream B: ui-ux-workflow.md → Frontend design  
Stream C: testing-workflow.md → Test strategy

MERGE POINT: feature-development.md → Integration
```

### **Quality Gate Pipeline**
```
QUALITY GATES:
1. architecture-workflow.md → Gate: Architecture review
2. feature-development.md → Gate: Code review
3. testing-workflow.md → Gate: Quality assurance
4. project-management.md → Gate: Release readiness
```

### **Feedback Loop Integration**
```
CONTINUOUS IMPROVEMENT:
1. feature-development.md → Implementation
2. testing-workflow.md → Validation & metrics
3. architecture-workflow.md → Process improvement
4. Repeat with learnings incorporated
```

## 🎯 Practical Examples

### **Example 1: New Feature with Unknown Complexity**
```
"Implement file export functionality using this workflow chain:

1. INVESTIGATE template from quick-templates.md
   → Analyze export requirements and complexity

2. IF complex: architecture-workflow.md phases 1-3
   → Design export architecture
   
3. ui-ux-workflow.md phases 1-3  
   → Design export interface using architecture from step 2

4. feature-development.md
   → Implement using designs from steps 2-3

5. testing-workflow.md
   → Comprehensive testing of export functionality"
```

### **Example 2: Performance Issue Resolution**
```
"Resolve application startup performance using:

1. bug-fix-workflow.md phases 1-2
   → Identify performance bottlenecks
   
2. architecture-workflow.md phases 3-4
   → Design performance optimization strategy using findings from step 1
   
3. PERFORMANCE FIX template from quick-templates.md
   → Implement optimizations based on strategy from step 2
   
4. testing-workflow.md phase 5
   → Validate performance improvements"
```

### **Example 3: UI Modernization Project**
```
"Modernize the application interface using:

1. ui-ux-workflow.md phases 1-2
   → Research modern UI patterns and user needs
   
2. architecture-workflow.md phases 3-4  
   → Design component architecture using research from step 1
   
3. feature-development.md phases 3-4
   → Implement component system based on architecture from step 2
   
4. ui-ux-workflow.md phases 4-6
   → Apply new components to existing interfaces
   
5. testing-workflow.md
   → Comprehensive usability and regression testing"
```

## 💡 Best Practices

### **✅ Workflow Chaining DO's**
- **Define clear handoff points**: What artifacts pass between workflows
- **Maintain traceability**: Reference previous workflow decisions
- **Plan for iteration**: Allow feedback loops and refinement
- **Document dependencies**: Which workflows depend on others
- **Validate at gates**: Check quality before proceeding to next workflow

### **❌ Workflow Chaining DON'Ts**
- **Skip validation between workflows**: Always verify outputs before proceeding
- **Create circular dependencies**: Avoid workflows that depend on their own outputs
- **Ignore workflow context**: Consider how previous decisions impact later workflows
- **Rush through handoffs**: Take time to properly transfer artifacts and context

## 🚀 Usage Examples

### **Simple Chaining**
```
"Follow architecture-workflow.md phases 1-2, then use those results as input for feature-development.md to implement the data sync feature"
```

### **Complex Orchestration**
```
"Execute this workflow pipeline for the multi-platform deployment:
1. architecture-workflow.md → Platform strategy
2. Parallel: ui-ux-workflow.md + testing-workflow.md → Interface + test strategy  
3. feature-development.md → Implementation using all previous outputs
4. project-management.md → Coordinate final deployment"
```

### **Conditional Execution**
```
"Start with INVESTIGATE template, then:
- If architectural issue: architecture-workflow.md → feature-development.md
- If UI issue: ui-ux-workflow.md → quick-templates.md  
- If simple bug: bug-fix-workflow.md only"
```

---

## 🎯 Result

**Workflow composition gives you enterprise-grade development orchestration!** You can create sophisticated development pipelines that ensure quality, traceability, and systematic progress through complex initiatives.

**Remember**: The key is defining clear **inputs**, **outputs**, and **handoff points** between workflows!